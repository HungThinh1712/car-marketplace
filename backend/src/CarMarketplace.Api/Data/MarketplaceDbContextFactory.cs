using CarMarketplace.Api.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CarMarketplace.Api.Data;

/// <summary>Design-time factory used by <c>dotnet ef</c>. It reads <c>ConnectionStrings:Marketplace</c> from the environment or <c>.env</c>.</summary>
public sealed class MarketplaceDbContextFactory : IDesignTimeDbContextFactory<MarketplaceDbContext>
{
    /// <summary>Creates a context for migrations.</summary>
    public MarketplaceDbContext CreateDbContext(string[] args)
    {
        EnvFile.Load();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Marketplace");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Marketplace is not configured.");
        }

        var options = new DbContextOptionsBuilder<MarketplaceDbContext>();
        MarketplaceDbContext.Configure(options, connectionString);
        return new MarketplaceDbContext(options.Options);
    }
}
