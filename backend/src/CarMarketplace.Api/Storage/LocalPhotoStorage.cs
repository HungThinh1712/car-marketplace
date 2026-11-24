using CarMarketplace.Api.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace CarMarketplace.Api.Storage;

/// <summary>Stores re-encoded photos on the local disk, outside <c>wwwroot</c>.</summary>
public sealed class LocalPhotoStorage : IPhotoStorage
{
    private readonly string _root;

    /// <summary>Creates storage rooted at the configured directory.</summary>
    public LocalPhotoStorage(string root)
    {
        _root = root;
        Directory.CreateDirectory(_root);
    }

    /// <summary>Accepts JPEG, PNG, and WebP only, then re-encodes so EXIF and other metadata are removed.</summary>
    public PreparedPhoto Prepare(Stream content, long length)
    {
        if (length > PhotoRules.MaxBytes)
        {
            throw PhotoError(PhotoRules.SizeMessage);
        }

        if (length <= 0)
        {
            throw PhotoError(PhotoRules.TypeMessage);
        }

        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        if (buffer.Length is <= 0 or > PhotoRules.MaxBytes)
        {
            throw PhotoError(buffer.Length > PhotoRules.MaxBytes ? PhotoRules.SizeMessage : PhotoRules.TypeMessage);
        }

        buffer.Position = 0;
        IImageFormat? format;
        try
        {
            format = Image.DetectFormat(buffer);
        }
        catch (UnknownImageFormatException)
        {
            throw PhotoError(PhotoRules.TypeMessage);
        }

        var kind = format?.Name?.ToUpperInvariant();
        if (kind is not ("JPEG" or "PNG" or "WEBP"))
        {
            throw PhotoError(PhotoRules.TypeMessage);
        }

        buffer.Position = 0;
        using var image = LoadImage(buffer);
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;

        using var output = new MemoryStream();
        string contentType;
        string extension;
        switch (kind)
        {
            case "JPEG":
                image.Save(output, new JpegEncoder());
                contentType = "image/jpeg";
                extension = ".jpg";
                break;
            case "PNG":
                image.Save(output, new PngEncoder());
                contentType = "image/png";
                extension = ".png";
                break;
            default:
                image.Save(output, new WebpEncoder());
                contentType = "image/webp";
                extension = ".webp";
                break;
        }

        if (output.Length is <= 0 or > PhotoRules.MaxBytes)
        {
            throw PhotoError(PhotoRules.SizeMessage);
        }

        return new PreparedPhoto(output.ToArray(), contentType, extension);
    }

    /// <summary>Writes a generated file name under the storage root.</summary>
    public async Task WriteAsync(string storedFileName, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_root);
        var path = SafePath(storedFileName);
        await File.WriteAllBytesAsync(path, bytes.ToArray(), cancellationToken);
    }

    /// <summary>Opens a stored file.</summary>
    public Stream? OpenRead(string storedFileName)
    {
        var path = SafePath(storedFileName);
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    /// <summary>Deletes a stored file when it exists.</summary>
    public void Delete(string storedFileName)
    {
        var path = SafePath(storedFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string SafePath(string storedFileName)
    {
        var name = Path.GetFileName(storedFileName);
        if (!string.Equals(name, storedFileName, StringComparison.Ordinal) || name.Length == 0)
        {
            throw new InvalidOperationException("Invalid photo name.");
        }

        return Path.Combine(_root, name);
    }

    private static Image LoadImage(Stream content)
    {
        try
        {
            return Image.Load(content);
        }
        catch (UnknownImageFormatException)
        {
            throw PhotoError(PhotoRules.TypeMessage);
        }
        catch (InvalidImageContentException)
        {
            throw PhotoError(PhotoRules.TypeMessage);
        }
    }

    private static FieldValidationException PhotoError(string message) =>
        new("The listing was not saved.", new Dictionary<string, string[]>
        {
            ["photos"] = [message]
        });
}
