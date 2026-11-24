using CarMarketplace.Api.Entities;

namespace CarMarketplace.Api.Contracts;

/// <summary>A car in the public browse list.</summary>
/// <param name="Id">Listing id.</param>
/// <param name="Make">Make.</param>
/// <param name="Model">Model.</param>
/// <param name="Year">Model year.</param>
/// <param name="Price">Asking price in USD.</param>
/// <param name="CurrencyCode">Always USD.</param>
/// <param name="Mileage">Odometer reading in kilometers.</param>
/// <param name="MileageUnit">Always km.</param>
/// <param name="Location">City and region.</param>
/// <param name="Status">Available or Sold.</param>
/// <param name="CoverPhotoUrl">URL of the cover photo.</param>
/// <param name="PublishedAt">Original publication time. Edits do not change it.</param>
public sealed record ListingSummary(
    string Id,
    string Make,
    string Model,
    int Year,
    decimal Price,
    string CurrencyCode,
    int Mileage,
    string MileageUnit,
    string Location,
    ListingStatus Status,
    string CoverPhotoUrl,
    DateTimeOffset PublishedAt);

/// <summary>One photo on a listing, ordered by sort order. The cover is sort order 0.</summary>
/// <param name="Id">Photo id.</param>
/// <param name="Url">URL that returns the image bytes.</param>
/// <param name="SortOrder">Zero-based order. Zero is the cover.</param>
public sealed record ListingPhotoResponse(string Id, string Url, int SortOrder);

/// <summary>A listing returned by detail, publish, and edit. It does not include the seller email, phone, or street address.</summary>
/// <param name="Id">Listing id.</param>
/// <param name="Make">Make.</param>
/// <param name="Model">Model.</param>
/// <param name="Year">Model year.</param>
/// <param name="Price">Asking price in USD.</param>
/// <param name="CurrencyCode">Always USD.</param>
/// <param name="Mileage">Odometer reading in kilometers.</param>
/// <param name="MileageUnit">Always km.</param>
/// <param name="Location">City and region.</param>
/// <param name="Description">Seller's description.</param>
/// <param name="SellerDisplayName">Seller display name.</param>
/// <param name="Status">Available or Sold.</param>
/// <param name="PublishedAt">Original publication time.</param>
/// <param name="UpdatedAt">Time of the latest successful save.</param>
/// <param name="Photos">Photos ordered by sort order.</param>
public sealed record ListingResponse(
    string Id,
    string Make,
    string Model,
    int Year,
    decimal Price,
    string CurrencyCode,
    int Mileage,
    string MileageUnit,
    string Location,
    string Description,
    string SellerDisplayName,
    ListingStatus Status,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ListingPhotoResponse> Photos);

/// <summary>One page of listings.</summary>
/// <param name="Items">Listings on this page. Empty when the page is past the end.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Always 20.</param>
/// <param name="TotalCount">Matching listings across all pages, including a page past the end.</param>
public sealed record ListingListResponse(
    IReadOnlyList<ListingSummary> Items,
    int Page,
    int PageSize,
    int TotalCount);
