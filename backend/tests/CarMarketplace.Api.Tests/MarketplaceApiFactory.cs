using CarMarketplace.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CarMarketplace.Api.Tests;

public sealed class MarketplaceApiFactory : WebApplicationFactory<Program>
{
    public string PhotoRoot { get; } = Path.Combine(Path.GetTempPath(), "car-marketplace-tests", Guid.NewGuid().ToString("N"));

    public MarketplaceApiFactory()
    {
        Directory.CreateDirectory(PhotoRoot);
        // Program reads these before the factory's configuration callbacks run.
        Environment.SetEnvironmentVariable("ConnectionStrings__Marketplace", TestDatabase.ConnectionString);
        Environment.SetEnvironmentVariable("PhotoStorage__RootPath", PhotoRoot);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Marketplace"] = TestDatabase.ConnectionString,
                ["PhotoStorage:RootPath"] = PhotoRoot,
                ["WebOrigin"] = "http://localhost:3000"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(PhotoRoot))
        {
            Directory.Delete(PhotoRoot, recursive: true);
        }
    }
}

public static class TestDatabase
{
    public static string ConnectionString { get; } = Build();

    private static string Build()
    {
        EnvFile.Load();
        var source = Environment.GetEnvironmentVariable("ConnectionStrings__Marketplace");
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new InvalidOperationException("ConnectionStrings:Marketplace is not configured.");
        }

        return source.Replace("Database=car_marketplace;", "Database=car_marketplace_test;", StringComparison.Ordinal);
    }
}
