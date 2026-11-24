namespace CarMarketplace.Api.Entities;

/// <summary>One message from a buyer about one listing. Not a thread.</summary>
public sealed class Inquiry
{
    /// <summary>Primary key.</summary>
    public string Id { get; set; } = "";

    /// <summary>Listing this inquiry is about.</summary>
    public string ListingId { get; set; } = "";

    /// <summary>Listing navigation.</summary>
    public Listing Listing { get; set; } = null!;

    /// <summary>Name the buyer entered.</summary>
    public string BuyerName { get; set; } = "";

    /// <summary>Email the buyer entered.</summary>
    public string BuyerEmail { get; set; } = "";

    /// <summary>Optional phone the buyer entered.</summary>
    public string? BuyerPhone { get; set; }

    /// <summary>The buyer's message. There is no reply column.</summary>
    public string Message { get; set; } = "";

    /// <summary>UTC time the inquiry was stored.</summary>
    public DateTime SubmittedAt { get; set; }
}
