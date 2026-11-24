using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Data;
using CarMarketplace.Api.Entities;
using CarMarketplace.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CarMarketplace.Api.Services;

/// <summary>Accepts and lists inquiries. There is no reply operation.</summary>
public interface IInquiryService
{
    /// <summary>Stores one inquiry about someone else's available listing.</summary>
    Task<InquiryCreatedResponse> CreateAsync(string? sellerId, string listingId, CreateInquiryRequest request, CancellationToken cancellationToken);

    /// <summary>Returns the owner's inquiries, newest first.</summary>
    Task<InquiryListResponse> ListForOwnerAsync(string sellerId, string listingId, CancellationToken cancellationToken);
}

/// <summary>Inquiry rules. A buyer may send more than one inquiry. Nothing is stored when the request is refused.</summary>
public sealed class InquiryService(MarketplaceDbContext db, IClock clock) : IInquiryService
{
    /// <summary>Validates and stores an inquiry.</summary>
    public async Task<InquiryCreatedResponse> CreateAsync(
        string? sellerId,
        string listingId,
        CreateInquiryRequest request,
        CancellationToken cancellationToken)
    {
        var listing = await db.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == listingId, cancellationToken);
        if (listing is null)
        {
            throw new NotFoundException("That listing was not found.");
        }

        if (sellerId is not null && string.Equals(listing.SellerAccountId, sellerId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("You cannot send an inquiry about your own listing.");
        }

        if (listing.Status != ListingStatus.Available)
        {
            throw new ConflictException("This car is sold and is not accepting inquiries.");
        }

        var errors = new ValidationErrors();
        if (!ContactRules.IsValidName(request.Name))
        {
            errors.Add("name", ContactRules.NameMessage);
        }

        if (!ContactRules.IsValidEmail(request.Email))
        {
            errors.Add("email", ContactRules.EmailMessage);
        }

        var message = request.Message?.Trim() ?? "";
        if (message.Length is < 10 or > 1000)
        {
            errors.Add("message", ContactRules.MessageMessage);
        }

        if (!ContactRules.IsValidOptionalPhone(request.Phone))
        {
            errors.Add("phone", ContactRules.PhoneMessage);
        }

        errors.ThrowIfAny("The inquiry was not saved.");

        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid().ToString("D"),
            ListingId = listing.Id,
            BuyerName = request.Name!.Trim(),
            BuyerEmail = request.Email!.Trim(),
            BuyerPhone = phone,
            Message = message,
            SubmittedAt = clock.UtcNow.UtcDateTime
        };
        db.Inquiries.Add(inquiry);
        await db.SaveChangesAsync(cancellationToken);
        return new InquiryCreatedResponse(
            inquiry.Id,
            listing.Id,
            listing.Make,
            listing.Model,
            listing.Year,
            ListingMapper.ToOffset(inquiry.SubmittedAt));
    }

    /// <summary>Lists inquiries for a listing the caller owns.</summary>
    public async Task<InquiryListResponse> ListForOwnerAsync(string sellerId, string listingId, CancellationToken cancellationToken)
    {
        var listing = await db.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == listingId, cancellationToken);
        if (listing is null)
        {
            throw new NotFoundException("That listing was not found.");
        }

        if (!string.Equals(listing.SellerAccountId, sellerId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("Only the owner can change this listing.");
        }

        var rows = await db.Inquiries
            .AsNoTracking()
            .Where(inquiry => inquiry.ListingId == listingId)
            .OrderByDescending(inquiry => inquiry.SubmittedAt)
            .ThenByDescending(inquiry => inquiry.Id)
            .ToListAsync(cancellationToken);
        var items = rows.Select(inquiry => new InquiryResponse(
            inquiry.Id,
            inquiry.BuyerName,
            inquiry.BuyerEmail,
            inquiry.BuyerPhone,
            inquiry.Message,
            ListingMapper.ToOffset(inquiry.SubmittedAt))).ToArray();
        return new InquiryListResponse(items);
    }
}
