namespace CarMarketplace.Api.Storage;

/// <summary>Re-encoded still image held in memory until the listing transaction commits.</summary>
/// <param name="Bytes">Image bytes with metadata removed.</param>
/// <param name="ContentType">image/jpeg, image/png, or image/webp.</param>
/// <param name="Extension">File extension including the leading dot.</param>
public sealed record PreparedPhoto(byte[] Bytes, string ContentType, string Extension);
