using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Data;
using CarMarketplace.Api.Entities;
using CarMarketplace.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace CarMarketplace.Api.Services;

/// <summary>Which listings a browse request includes.</summary>
public enum ListingStatusFilter
{
    /// <summary>Available listings. This is the default.</summary>
    Available,

    /// <summary>Sold listings.</summary>
    Sold,

    /// <summary>Available and sold listings. The query value is <c>all</c>.</summary>
    All
}

/// <summary>Reads the public catalog and photo bytes.</summary>
public interface IListingBrowseService
{
    /// <summary>Returns one page of listings. Filters and sort use status, published_at, and id so the browse index can serve the default page.</summary>
    Task<ListingListResponse> SearchAsync(
        string? make,
        string? model,
        string? location,
        int? minYear,
        int? maxYear,
        decimal? minPrice,
        decimal? maxPrice,
        int? maxMileage,
        ListingStatusFilter status,
        int page,
        CancellationToken cancellationToken);

    /// <summary>Returns one listing, available or sold.</summary>
    Task<ListingResponse?> GetAsync(string id, CancellationToken cancellationToken);

    /// <summary>Opens a photo by id.</summary>
    Task<PhotoContent?> OpenPhotoAsync(string id, CancellationToken cancellationToken);
}

/// <summary>Photo bytes and the content type to send.</summary>
/// <param name="Stream">File stream. The caller owns disposal through the response.</param>
/// <param name="ContentType">Image content type.</param>
public sealed record PhotoContent(Stream Stream, string ContentType);

/// <summary>Browse queries. The default list is available cars, newest published first, 20 per page.</summary>
public sealed class ListingBrowseService(MarketplaceDbContext db, IPhotoStorage storage) : IListingBrowseService
{
    /// <summary>Filters, sorts, and pages listings.</summary>
    public async Task<ListingListResponse> SearchAsync(
        string? make,
        string? model,
        string? location,
        int? minYear,
        int? maxYear,
        decimal? minPrice,
        decimal? maxPrice,
        int? maxMileage,
        ListingStatusFilter status,
        int page,
        CancellationToken cancellationToken)
    {
        var query = db.Listings.AsNoTracking();
        if (status == ListingStatusFilter.Available)
        {
            query = query.Where(listing => listing.Status == ListingStatus.Available);
        }
        else if (status == ListingStatusFilter.Sold)
        {
            query = query.Where(listing => listing.Status == ListingStatus.Sold);
        }

        if (!string.IsNullOrWhiteSpace(make))
        {
            var pattern = LikePattern(make);
            query = query.Where(listing => EF.Functions.Like(listing.Make, pattern));
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            var pattern = LikePattern(model);
            query = query.Where(listing => EF.Functions.Like(listing.Model, pattern));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var pattern = LikePattern(location);
            query = query.Where(listing => EF.Functions.Like(listing.Location, pattern));
        }

        if (minYear is int minimumYear)
        {
            query = query.Where(listing => listing.Year >= minimumYear);
        }

        if (maxYear is int maximumYear)
        {
            query = query.Where(listing => listing.Year <= maximumYear);
        }

        if (minPrice is decimal minimumPrice)
        {
            query = query.Where(listing => listing.Price >= minimumPrice);
        }

        if (maxPrice is decimal maximumPrice)
        {
            query = query.Where(listing => listing.Price <= maximumPrice);
        }

        if (maxMileage is int maximumMileage)
        {
            query = query.Where(listing => listing.MileageKm <= maximumMileage);
        }

        query = query
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

        var items = rows.Select(ListingMapper.ToSummary).ToArray();
        return ListingMapper.ToList(items, page, totalCount);
    }

    /// <summary>Loads one listing by id.</summary>
    public async Task<ListingResponse?> GetAsync(string id, CancellationToken cancellationToken)
    {
        var listing = await db.Listings
            .AsNoTracking()
            .Include(item => item.Photos)
            .Include(item => item.SellerAccount)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return listing is null ? null : ListingMapper.ToResponse(listing);
    }

    /// <summary>Opens the stored file for a photo id.</summary>
    public async Task<PhotoContent?> OpenPhotoAsync(string id, CancellationToken cancellationToken)
    {
        var photo = await db.ListingPhotos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (photo is null)
        {
            return null;
        }

        var stream = storage.OpenRead(photo.StoredFileName);
        return stream is null ? null : new PhotoContent(stream, photo.ContentType);
    }

    private static string LikePattern(string value)
    {
        var escaped = value.Trim()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
        return "%" + escaped + "%";
    }
}
