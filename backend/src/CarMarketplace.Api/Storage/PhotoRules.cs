namespace CarMarketplace.Api.Storage;

/// <summary>Photo limits and the sentences returned when a photo is rejected.</summary>
public static class PhotoRules
{
    /// <summary>Maximum stored photo size, 8 MB.</summary>
    public const int MaxBytes = 8 * 1024 * 1024;

    /// <summary>Maximum photos on one listing.</summary>
    public const int MaxCount = 10;

    /// <summary>Returned when the photo count is outside 1 to 10.</summary>
    public const string CountMessage = "Add 1 to 10 still photos. The first photo is the cover.";

    /// <summary>Returned for GIF, BMP, HEIC, video, and any other non JPEG, PNG, or WebP payload.</summary>
    public const string TypeMessage = "Each photo must be a still image (JPEG, PNG, or WebP). Video is not accepted.";

    /// <summary>Returned when a photo is larger than 8 MB.</summary>
    public const string SizeMessage = "Each photo must be 8 MB or smaller.";
}
