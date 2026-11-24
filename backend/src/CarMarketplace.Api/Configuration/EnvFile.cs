namespace CarMarketplace.Api.Configuration;

/// <summary>Loads the marketplace <c>.env</c> file into process environment variables.</summary>
public static class EnvFile
{
    /// <summary>
    /// Applies the nearest <c>.env</c> that contains <c>ConnectionStrings__Marketplace</c>.
    /// Existing environment variables are left unchanged.
    /// </summary>
    public static void Load()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate) && ContainsMarketplaceConnection(candidate))
            {
                Apply(candidate);
                return;
            }

            directory = directory.Parent;
        }
    }

    private static bool ContainsMarketplaceConnection(string path)
    {
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("ConnectionStrings__Marketplace=", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void Apply(string path)
    {
        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
