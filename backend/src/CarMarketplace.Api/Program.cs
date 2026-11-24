using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CarMarketplace.Api.Configuration;
using CarMarketplace.Api.Data;
using CarMarketplace.Api.Endpoints;
using CarMarketplace.Api.Middleware;
using CarMarketplace.Api.Services;
using CarMarketplace.Api.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

EnvFile.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Marketplace");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Marketplace is not configured.");
}

var photoRoot = builder.Configuration["PhotoStorage:RootPath"];
if (string.IsNullOrWhiteSpace(photoRoot))
{
    photoRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "storage", "photos"));
}
else if (!Path.IsPathRooted(photoRoot))
{
    photoRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, photoRoot));
}

builder.Services.AddDbContext<MarketplaceDbContext>(options => MarketplaceDbContext.Configure(options, connectionString));
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
    options.SerializerOptions.AllowDuplicateProperties = false;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IPhotoStorage>(_ => new LocalPhotoStorage(photoRoot));
builder.Services.AddScoped<ISellerAccountService, SellerAccountService>();
builder.Services.AddScoped<IListingBrowseService, ListingBrowseService>();
builder.Services.AddScoped<IListingCommandService, ListingCommandService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

var permitLimit = builder.Environment.IsEnvironment("Testing") ? 10_000 : 10;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await ApiProblems.WriteAsync(
            context.HttpContext.Response,
            context.HttpContext.Request,
            StatusCodes.Status429TooManyRequests,
            "Too Many Requests",
            "Too many requests. Try again in a minute.",
            cancellationToken);
    };
    options.AddPolicy("abuse", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services
    .AddAuthentication(AuthSchemes.Session)
    .AddCookie(AuthSchemes.Session, options =>
    {
        options.Cookie.Name = AuthSchemes.Session;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = false;
        options.ExpireTimeSpan = AuthSchemes.SessionLifetime;
        options.Events.OnRedirectToLogin = context =>
            ApiProblems.WriteAsync(context.Response, context.Request, StatusCodes.Status401Unauthorized, "Unauthorized", "Sign in to continue.");
        options.Events.OnRedirectToAccessDenied = context =>
            ApiProblems.WriteAsync(context.Response, context.Request, StatusCodes.Status403Forbidden, "Forbidden", "Only the owner can change this listing.");
    });
builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<OriginCheckMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapSellerEndpoints();
app.MapSessionEndpoints();
app.MapListingEndpoints();
app.MapPhotoEndpoints();
app.MapInquiryEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

/// <summary>Car marketplace HTTP API.</summary>
public partial class Program;
