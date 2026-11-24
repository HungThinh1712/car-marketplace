namespace CarMarketplace.Api.Storage;

/// <summary>Reads and writes listing photos outside the web root.</summary>
public interface IPhotoStorage
{
    /// <summary>Decodes a still image and re-encodes it without metadata. Does not write a file.</summary>
    PreparedPhoto Prepare(Stream content, long length);

    /// <summary>Writes prepared bytes under a generated file name.</summary>
    Task WriteAsync(string storedFileName, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken);

    /// <summary>Opens a stored file, or returns null when it is missing.</summary>
    Stream? OpenRead(string storedFileName);

    /// <summary>Deletes a stored file when it exists.</summary>
    void Delete(string storedFileName);
}
