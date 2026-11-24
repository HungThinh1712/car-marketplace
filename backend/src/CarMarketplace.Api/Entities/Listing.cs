namespace CarMarketplace.Api.Entities;

/// <summary>One car offered by one seller.</summary>
public sealed class Listing
{
    /// <summary>Primary key.</summary>
    public string Id { get; set; } = "";

    /// <summary>Owner of the listing.</summary>
    public string SellerAccountId { get; set; } = "";

    /// <summary>Owner navigation.</summary>
    public SellerAccount SellerAccount { get; set; } = null!;

    /// <summary>Make, 1 to 50 characters.</summary>
    public string Make { get; set; } = "";

    /// <summary>Model, 1 to 50 characters.</summary>
    public string Model { get; set; } = "";

    /// <summary>Model year.</summary>
    public short Year { get; set; }

    /// <summary>Asking price in USD.</summary>
    public decimal Price { get; set; }

    /// <summary>Odometer reading in kilometers.</summary>
    public int MileageKm { get; set; }

    /// <summary>City and region text.</summary>
    public string Location { get; set; } = "";

    /// <summary>Seller's description of the car.</summary>
    public string Description { get; set; } = "";

    /// <summary>Available or Sold.</summary>
    public ListingStatus Status { get; set; }

    /// <summary>UTC time the listing was first published. Edits do not change it.</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>UTC time of the latest successful save.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Photos for this listing. Sort order 0 is the cover.</summary>
    public List<ListingPhoto> Photos { get; set; } = [];

    /// <summary>Inquiries sent about this listing.</summary>
    public List<Inquiry> Inquiries { get; set; } = [];
}
