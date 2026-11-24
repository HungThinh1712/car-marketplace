using CarMarketplace.Api.Entities;

namespace CarMarketplace.Api.Contracts;

/// <summary>Replacement facts for a listing. Photos, status, and the original publication time stay as they are.</summary>
public sealed record ReplaceListingRequest
{
    /// <summary>Make, 1 to 50 characters.</summary>
    public string? Make { get; init; }

    /// <summary>Model, 1 to 50 characters.</summary>
    public string? Model { get; init; }

    /// <summary>Model year.</summary>
    public int? Year { get; init; }

    /// <summary>Asking price in USD.</summary>
    public decimal? Price { get; init; }

    /// <summary>Odometer reading in kilometers.</summary>
    public int? Mileage { get; init; }

    /// <summary>City and region.</summary>
    public string? Location { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }
}

/// <summary>Status change. The only values are Available and Sold.</summary>
public sealed record ChangeListingStatusRequest
{
    /// <summary>Available or Sold.</summary>
    public ListingStatus? Status { get; init; }
}
