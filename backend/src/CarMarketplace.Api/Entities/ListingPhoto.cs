namespace CarMarketplace.Api.Entities;

/// <summary>A still image attached to one listing.</summary>
public sealed class ListingPhoto
{
    /// <summary>Primary key. Also used in the stored file name.</summary>
    public string Id { get; set; } = "";

    /// <summary>Listing this photo belongs to.</summary>
    public string ListingId { get; set; } = "";

    /// <summary>Listing navigation.</summary>
    public Listing Listing { get; set; } = null!;

    /// <summary>Zero-based order. Zero is the cover.</summary>
    public byte SortOrder { get; set; }

    /// <summary>Content type after re-encode: image/jpeg, image/png, or image/webp.</summary>
    public string ContentType { get; set; } = "";

    /// <summary>Generated file name. The original upload name is discarded.</summary>
    public string StoredFileName { get; set; } = "";

    /// <summary>Size of the stored bytes.</summary>
    public int ByteSize { get; set; }
}
