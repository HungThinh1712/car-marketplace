using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Data;
using CarMarketplace.Api.Entities;
using CarMarketplace.Api.Exceptions;
using CarMarketplace.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace CarMarketplace.Api.Services;

/// <summary>Publishes and updates listings for the signed-in owner.</summary>
public interface IListingCommandService
{
    /// <summary>Publishes one available listing and its photos, or saves nothing.</summary>
    Task<ListingResponse> PublishAsync(string sellerId, ListingFacts facts, IReadOnlyList<IFormFile> photos, CancellationToken cancellationToken);

    /// <summary>Replaces facts without changing photos, status, or the original publication time.</summary>
    Task<ListingResponse> ReplaceFactsAsync(string sellerId, string listingId, ListingFacts facts, CancellationToken cancellationToken);

    /// <summary>Replaces the whole photo set. A rejected set leaves the previous photos in place.</summary>
    Task<ListingResponse> ReplacePhotosAsync(string sellerId, string listingId, IReadOnlyList<IFormFile> photos, CancellationToken cancellationToken);

    /// <summary>Moves a listing between Available and Sold without changing the original publication time.</summary>
    Task<ListingResponse> ChangeStatusAsync(string sellerId, string listingId, ListingStatus? status, CancellationToken cancellationToken);

    /// <summary>Lists the caller's listings, including sold cars.</summary>
    Task<ListingListResponse> ListMineAsync(string sellerId, int page, CancellationToken cancellationToken);
}

/// <summary>Listing writes. A refused publish leaves no listing row and no leftover file.</summary>
public sealed class ListingCommandService(
    MarketplaceDbContext db,
    IPhotoStorage storage,
    IClock clock) : IListingCommandService
{
    /// <summary>Saves a new available listing and its photos in one transaction.</summary>
    public async Task<ListingResponse> PublishAsync(
        string sellerId,
        ListingFacts facts,
        IReadOnlyList<IFormFile> photos,
        CancellationToken cancellationToken)
    {
        var seller = await RequireSellerAsync(sellerId, cancellationToken);
        var errors = new ValidationErrors();
        ListingRules.Validate(errors, facts, clock);
        if (photos.Count is < 1 or > PhotoRules.MaxCount)
        {
            errors.Add("photos", PhotoRules.CountMessage);
        }

        errors.ThrowIfAny(ListingRules.NotSaved);
        var prepared = PrepareAll(photos);
        var normalized = Normalize(facts);
        var now = clock.UtcNow.UtcDateTime;
        var listing = new Listing
        {
            Id = Guid.NewGuid().ToString("D"),
            SellerAccountId = seller.Id,
            SellerAccount = seller,
            Make = normalized.Make,
            Model = normalized.Model,
            Year = normalized.Year,
            Price = normalized.Price,
            MileageKm = normalized.Mileage,
            Location = normalized.Location,
            Description = normalized.Description,
            Status = ListingStatus.Available,
            PublishedAt = now,
            UpdatedAt = now
        };
        var photoRows = BuildPhotos(listing.Id, prepared);
        foreach (var photo in photoRows)
        {
            photo.Listing = listing;
        }

        listing.Photos = photoRows;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            db.Listings.Add(listing);
            await db.SaveChangesAsync(cancellationToken);
            await WriteAllAsync(photoRows, prepared, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            DeleteAll(photoRows.Select(photo => photo.StoredFileName));
            throw;
        }

        return ListingMapper.ToResponse(listing);
    }

    /// <summary>Replaces facts. <c>published_at</c> is not updated.</summary>
    public async Task<ListingResponse> ReplaceFactsAsync(
        string sellerId,
        string listingId,
        ListingFacts facts,
        CancellationToken cancellationToken)
    {
        var listing = await RequireOwnedAsync(sellerId, listingId, cancellationToken);
        var errors = new ValidationErrors();
        ListingRules.Validate(errors, facts, clock);
        errors.ThrowIfAny(ListingRules.NotSaved);
        var normalized = Normalize(facts);
        listing.Make = normalized.Make;
        listing.Model = normalized.Model;
        listing.Year = normalized.Year;
        listing.Price = normalized.Price;
        listing.MileageKm = normalized.Mileage;
        listing.Location = normalized.Location;
        listing.Description = normalized.Description;
        listing.UpdatedAt = clock.UtcNow.UtcDateTime;
        db.Entry(listing).Property(item => item.PublishedAt).IsModified = false;
        await db.SaveChangesAsync(cancellationToken);
        return ListingMapper.ToResponse(listing);
    }

    /// <summary>Replaces photos. Zero files or an invalid file keeps the previous set.</summary>
    public async Task<ListingResponse> ReplacePhotosAsync(
        string sellerId,
        string listingId,
        IReadOnlyList<IFormFile> photos,
        CancellationToken cancellationToken)
    {
        var listing = await RequireOwnedAsync(sellerId, listingId, cancellationToken);
        if (photos.Count is < 1 or > PhotoRules.MaxCount)
        {
            throw new FieldValidationException(ListingRules.NotSaved, new Dictionary<string, string[]>
            {
                ["photos"] = [PhotoRules.CountMessage]
            });
        }

        var prepared = PrepareAll(photos);
        var oldNames = listing.Photos.Select(photo => photo.StoredFileName).ToList();
        var newPhotos = BuildPhotos(listing.Id, prepared);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            db.ListingPhotos.RemoveRange(listing.Photos);
            listing.Photos.Clear();
            foreach (var photo in newPhotos)
            {
                listing.Photos.Add(photo);
            }

            listing.UpdatedAt = clock.UtcNow.UtcDateTime;
            db.Entry(listing).Property(item => item.PublishedAt).IsModified = false;
            await db.SaveChangesAsync(cancellationToken);
            await WriteAllAsync(newPhotos, prepared, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            DeleteAll(newPhotos.Select(photo => photo.StoredFileName));
            throw;
        }

        DeleteAll(oldNames);
        return ListingMapper.ToResponse(listing);
    }

    /// <summary>Sets Available or Sold. Setting the current status returns the same listing.</summary>
    public async Task<ListingResponse> ChangeStatusAsync(
        string sellerId,
        string listingId,
        ListingStatus? status,
        CancellationToken cancellationToken)
    {
        var listing = await RequireOwnedAsync(sellerId, listingId, cancellationToken);
        if (status is not (ListingStatus.Available or ListingStatus.Sold))
        {
            throw new FieldValidationException(ListingRules.NotSaved, new Dictionary<string, string[]>
            {
                ["status"] = ["Status is the word Available or Sold."]
            });
        }

        if (listing.Status != status)
        {
            listing.Status = status.Value;
            listing.UpdatedAt = clock.UtcNow.UtcDateTime;
            db.Entry(listing).Property(item => item.PublishedAt).IsModified = false;
            await db.SaveChangesAsync(cancellationToken);
        }

        return ListingMapper.ToResponse(listing);
    }

    /// <summary>Returns the caller's listings, newest published first.</summary>
    public async Task<ListingListResponse> ListMineAsync(string sellerId, int page, CancellationToken cancellationToken)
    {
        await RequireSellerAsync(sellerId, cancellationToken);
        var query = db.Listings
            .AsNoTracking()
            .Where(listing => listing.SellerAccountId == sellerId)
            .OrderByDescending(listing => listing.PublishedAt)
            .ThenByDescending(listing => listing.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .Skip((page - 1) * ListingMapper.PageSize)
            .Take(ListingMapper.PageSize)
            .Include(listing => listing.Photos)
            .Include(listing => listing.SellerAccount)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return ListingMapper.ToList(rows.Select(ListingMapper.ToSummary).ToArray(), page, totalCount);
    }

    private async Task<SellerAccount> RequireSellerAsync(string sellerId, CancellationToken cancellationToken)
    {
        var seller = await db.SellerAccounts.FirstOrDefaultAsync(account => account.Id == sellerId, cancellationToken);
        if (seller is null)
        {
            throw new UnauthorizedException("Sign in to continue.");
        }

        return seller;
    }

    private async Task<Listing> RequireOwnedAsync(string sellerId, string listingId, CancellationToken cancellationToken)
    {
        var listing = await db.Listings
            .Include(item => item.Photos)
            .Include(item => item.SellerAccount)
            .FirstOrDefaultAsync(item => item.Id == listingId, cancellationToken);
        if (listing is null)
        {
            throw new NotFoundException("That listing was not found.");
        }

        if (!string.Equals(listing.SellerAccountId, sellerId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("Only the owner can change this listing.");
        }

        return listing;
    }

    private List<PreparedPhoto> PrepareAll(IReadOnlyList<IFormFile> photos)
    {
        var prepared = new List<PreparedPhoto>(photos.Count);
        foreach (var photo in photos)
        {
            using var stream = photo.OpenReadStream();
            prepared.Add(storage.Prepare(stream, photo.Length));
        }

        return prepared;
    }

    private static List<ListingPhoto> BuildPhotos(string listingId, IReadOnlyList<PreparedPhoto> prepared)
    {
        var photos = new List<ListingPhoto>(prepared.Count);
        for (var index = 0; index < prepared.Count; index++)
        {
            var item = prepared[index];
            var id = Guid.NewGuid().ToString("D");
            photos.Add(new ListingPhoto
            {
                Id = id,
                ListingId = listingId,
                SortOrder = (byte)index,
                ContentType = item.ContentType,
                StoredFileName = id + item.Extension,
                ByteSize = item.Bytes.Length
            });
        }

        return photos;
    }

    private async Task WriteAllAsync(
        IReadOnlyList<ListingPhoto> photos,
        IReadOnlyList<PreparedPhoto> prepared,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < photos.Count; index++)
        {
            await storage.WriteAsync(photos[index].StoredFileName, prepared[index].Bytes, cancellationToken);
        }
    }

    private void DeleteAll(IEnumerable<string> storedFileNames)
    {
        foreach (var name in storedFileNames)
        {
            storage.Delete(name);
        }
    }

    private static NormalizedFacts Normalize(ListingFacts facts) =>
        new(
            facts.Make!.Trim(),
            facts.Model!.Trim(),
            (short)facts.Year!.Value,
            facts.Price!.Value,
            facts.Mileage!.Value,
            facts.Location!.Trim(),
            facts.Description!.Trim());

    private readonly record struct NormalizedFacts(
        string Make,
        string Model,
        short Year,
        decimal Price,
        int Mileage,
        string Location,
        string Description);
}
