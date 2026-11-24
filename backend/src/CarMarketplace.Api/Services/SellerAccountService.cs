using System.Security.Claims;
using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Data;
using CarMarketplace.Api.Entities;
using CarMarketplace.Api.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CarMarketplace.Api.Services;

/// <summary>Registers sellers, signs them in, and signs them out.</summary>
public interface ISellerAccountService
{
    /// <summary>Creates an account and signs the caller in.</summary>
    Task<SellerResponse> RegisterAsync(RegisterSellerRequest request, CancellationToken cancellationToken);

    /// <summary>Signs in an existing seller. A mismatch signs nobody in.</summary>
    Task<SellerResponse> SignInAsync(SignInRequest request, CancellationToken cancellationToken);

    /// <summary>Clears the session cookie.</summary>
    Task SignOutAsync(CancellationToken cancellationToken);

    /// <summary>Returns the seller for a session id, or null when the account is gone.</summary>
    Task<SellerResponse?> GetCurrentAsync(string sellerId, CancellationToken cancellationToken);
}

/// <summary>Password hashing and the fixed 14-day session cookie.</summary>
public sealed class SellerAccountService : ISellerAccountService
{
    private readonly MarketplaceDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClock _clock;
    private readonly PasswordHasher<SellerAccount> _hasher = new();
    private readonly string _dummyHash;

    /// <summary>Creates the service.</summary>
    public SellerAccountService(MarketplaceDbContext db, IHttpContextAccessor httpContextAccessor, IClock clock)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
        var dummy = new SellerAccount
        {
            Id = "00000000-0000-0000-0000-000000000000",
            Email = "missing@example.com",
            DisplayName = "Missing"
        };
        _dummyHash = _hasher.HashPassword(dummy, "not-a-real-account-password");
    }

    /// <summary>Validates the account, stores the email in lower case, and issues the session cookie.</summary>
    public async Task<SellerResponse> RegisterAsync(RegisterSellerRequest request, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        if (!ContactRules.IsValidName(request.DisplayName))
        {
            errors.Add("displayName", ContactRules.DisplayNameMessage);
        }

        if (!ContactRules.IsValidEmail(request.Email))
        {
            errors.Add("email", ContactRules.EmailMessage);
        }

        if (!ContactRules.IsValidPassword(request.Password))
        {
            errors.Add("password", ContactRules.PasswordMessage);
        }

        errors.ThrowIfAny("The account was not saved.");

        var email = request.Email!.Trim().ToLowerInvariant();
        var displayName = request.DisplayName!.Trim();
        if (await _db.SellerAccounts.AnyAsync(account => account.Email == email, cancellationToken))
        {
            throw new ConflictException(ContactRules.DuplicateEmailMessage);
        }

        var now = _clock.UtcNow.UtcDateTime;
        var account = new SellerAccount
        {
            Id = Guid.NewGuid().ToString("D"),
            Email = email,
            DisplayName = displayName,
            CreatedAt = now,
            LastSignedInAt = now
        };
        account.PasswordHash = _hasher.HashPassword(account, request.Password!);
        _db.SellerAccounts.Add(account);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            throw new ConflictException(ContactRules.DuplicateEmailMessage);
        }

        await IssueSessionAsync(account);
        return ToResponse(account);
    }

    /// <summary>Checks the password and issues a new 14-day cookie that does not slide.</summary>
    public async Task<SellerResponse> SignInAsync(SignInRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? "";
        var password = request.Password ?? "";
        var account = await _db.SellerAccounts.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (account is null)
        {
            _hasher.VerifyHashedPassword(new SellerAccount(), _dummyHash, password);
            throw new UnauthorizedException(ContactRules.SignInMessage);
        }

        var verification = _hasher.VerifyHashedPassword(account, account.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(ContactRules.SignInMessage);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.PasswordHash = _hasher.HashPassword(account, password);
        }

        account.LastSignedInAt = _clock.UtcNow.UtcDateTime;
        await _db.SaveChangesAsync(cancellationToken);
        await IssueSessionAsync(account);
        return ToResponse(account);
    }

    /// <summary>Clears <c>marketplace_session</c>.</summary>
    public async Task SignOutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        await httpContext.SignOutAsync(AuthSchemes.Session);
    }

    /// <summary>Loads the signed-in seller.</summary>
    public async Task<SellerResponse?> GetCurrentAsync(string sellerId, CancellationToken cancellationToken)
    {
        var account = await _db.SellerAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == sellerId, cancellationToken);
        return account is null ? null : ToResponse(account);
    }

    private async Task IssueSessionAsync(SellerAccount account)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        var issued = _clock.UtcNow;
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, account.Id),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Name, account.DisplayName)
            ],
            AuthSchemes.Session);
        await httpContext.SignInAsync(
            AuthSchemes.Session,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = false,
                IssuedUtc = issued,
                ExpiresUtc = issued.Add(AuthSchemes.SessionLifetime)
            });
    }

    private static SellerResponse ToResponse(SellerAccount account) =>
        new(account.Id, account.DisplayName, account.Email);

    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        for (var current = exception.InnerException; current is not null; current = current.InnerException)
        {
            if (current is MySqlException mysql && mysql.Number == 1062)
            {
                return true;
            }
        }

        return false;
    }
}
