using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Entities;

namespace CarMarketplace.Api.Services;

/// <summary>Maps listing rows to API records. Seller email is not copied.</summary>
public static class ListingMapper
{
    /// <summary>Currency sent on every listing. It is not a column.</summary>
    public const string CurrencyCode = "USD";

    /// <summary>Mileage unit sent on every listing. It is not a column.</summary>
    public const string MileageUnit = "km";

    /// <summary>Fixed page size.</summary>
    public const int PageSize = 20;

    /// <summary>Maps a UTC database time to a <see cref="DateTimeOffset"/> with offset zero.</summary>
    public static DateTimeOffset ToOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeSpan.Zero);

    /// <summary>Public photo URL.</summary>
    public static string PhotoUrl(string photoId) => $"/api/v1/photos/{photoId}";

    /// <summary>Maps a listing and its cover photo.</summary>
    public static ListingSummary ToSummary(Listing listing)
    {
        var cover = listing.Photos.FirstOrDefault(photo => photo.SortOrder == 0);
        return new ListingSummary(
            listing.Id,
            listing.Make,
            listing.Model,
            listing.Year,
            listing.Price,
            CurrencyCode,
            listing.MileageKm,
            MileageUnit,
            listing.Location,
            listing.Status,
            cover is null ? "" : PhotoUrl(cover.Id),
            ToOffset(listing.PublishedAt));
    }

    /// <summary>Maps a listing, its seller display name, and its photos.</summary>
    public static ListingResponse ToResponse(Listing listing)
    {
        var photos = listing.Photos
            .OrderBy(photo => photo.SortOrder)
            .Select(photo => new ListingPhotoResponse(photo.Id, PhotoUrl(photo.Id), photo.SortOrder))
            .ToArray();
        return new ListingResponse(
            listing.Id,
            listing.Make,
            listing.Model,
            listing.Year,
            listing.Price,
            CurrencyCode,
            listing.MileageKm,
            MileageUnit,
            listing.Location,
            listing.Description,
            listing.SellerAccount.DisplayName,
            listing.Status,
            ToOffset(listing.PublishedAt),
            ToOffset(listing.UpdatedAt),
            photos);
    }

    /// <summary>Wraps one page. <paramref name="totalCount"/> is the full match count, even when <paramref name="items"/> is empty.</summary>
    public static ListingListResponse ToList(IReadOnlyList<ListingSummary> items, int page, int totalCount) =>
        new(items, page, PageSize, totalCount);
}
