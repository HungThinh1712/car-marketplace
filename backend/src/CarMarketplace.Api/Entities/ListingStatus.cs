namespace CarMarketplace.Api.Entities;

/// <summary>Whether a listing is for sale or already sold.</summary>
public enum ListingStatus
{
    /// <summary>The car is for sale.</summary>
    Available = 0,

    /// <summary>The car has been sold.</summary>
    Sold = 1
}
