using System.Net;
using System.Text.Json;
using CarMarketplace.Api.Data;
using CarMarketplace.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarMarketplace.Api.Tests;

[TestClass]
public sealed class BrowseListingsTests
{
    private MarketplaceApiFactory _factory = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new MarketplaceApiFactory();
        _client = ListingRequests.CreateClient(_factory);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Database.MigrateAsync();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task GetListings_DefaultBrowse_ReturnsOnlyTheAvailableCar()
    {
        var email = $"browse-{Guid.NewGuid():N}@example.com";
        var availableId = Guid.NewGuid().ToString("D");
        var soldId = Guid.NewGuid().ToString("D");
        var sellerId = Guid.NewGuid().ToString("D");
        await SeedAsync(sellerId, email, availableId, soldId);

        try
        {
            var listResponse = await _client.GetAsync("/api/v1/listings");
            Assert.AreEqual(HttpStatusCode.OK, listResponse.StatusCode);
            using var list = await ListingRequests.ReadAsync(listResponse);
            var root = list.RootElement;
            Assert.AreEqual(20, root.GetProperty("pageSize").GetInt32());
            Assert.AreEqual(1, root.GetProperty("totalCount").GetInt32());
            var items = root.GetProperty("items");
            Assert.HasCount(1, items.EnumerateArray().ToList());
            var item = items[0];
            Assert.AreEqual(availableId, item.GetProperty("id").GetString());
            Assert.AreEqual("Available", item.GetProperty("status").GetString());
            Assert.AreEqual("Honda", item.GetProperty("make").GetString());

            var detailResponse = await _client.GetAsync($"/api/v1/listings/{availableId}");
            Assert.AreEqual(HttpStatusCode.OK, detailResponse.StatusCode);
            var detailJson = await detailResponse.Content.ReadAsStringAsync();
            Assert.IsFalse(detailJson.Contains(email, StringComparison.OrdinalIgnoreCase));
            using var detail = JsonDocument.Parse(detailJson);
            var car = detail.RootElement;
            Assert.AreEqual("Honda", car.GetProperty("make").GetString());
            Assert.AreEqual("Civic", car.GetProperty("model").GetString());
            Assert.AreEqual(2018, car.GetProperty("year").GetInt32());
            Assert.AreEqual(18000.50m, car.GetProperty("price").GetDecimal());
            Assert.AreEqual("USD", car.GetProperty("currencyCode").GetString());
            Assert.AreEqual(42000, car.GetProperty("mileage").GetInt32());
            Assert.AreEqual("km", car.GetProperty("mileageUnit").GetString());
            Assert.AreEqual("Austin, TX", car.GetProperty("location").GetString());
            Assert.AreEqual("A commuter with service records.", car.GetProperty("description").GetString());
            Assert.AreEqual("North Yard Motors", car.GetProperty("sellerDisplayName").GetString());
            Assert.AreEqual("Available", car.GetProperty("status").GetString());
            Assert.IsGreaterThan(0, car.GetProperty("photos").GetArrayLength());
            Assert.IsFalse(car.TryGetProperty("email", out _));
            Assert.IsFalse(car.TryGetProperty("sellerEmail", out _));
        }
        finally
        {
            await DeleteSellerAsync(sellerId);
        }
    }

    private async Task SeedAsync(string sellerId, string email, string availableId, string soldId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Inquiries.ExecuteDeleteAsync();
        await db.ListingPhotos.ExecuteDeleteAsync();
        await db.Listings.ExecuteDeleteAsync();

        var now = DateTime.UtcNow;
        var photoId = Guid.NewGuid().ToString("D");
        var fileName = photoId + ".png";
        await File.WriteAllBytesAsync(Path.Combine(_factory.PhotoRoot, fileName), ListingRequests.Png);
        var seller = new SellerAccount
        {
            Id = sellerId,
            Email = email,
            DisplayName = "North Yard Motors",
            PasswordHash = "not-used-for-browse",
            CreatedAt = now,
            LastSignedInAt = now
        };
        db.SellerAccounts.Add(seller);
        db.Listings.Add(ListingRow(availableId, seller, ListingStatus.Available, now, photoId, fileName));
        db.Listings.Add(ListingRow(soldId, seller, ListingStatus.Sold, now.AddMinutes(-10), null, null));
        await db.SaveChangesAsync();
    }

    private static Listing ListingRow(
        string id,
        SellerAccount seller,
        ListingStatus status,
        DateTime publishedAt,
        string? photoId,
        string? fileName)
    {
        var listing = new Listing
        {
            Id = id,
            SellerAccountId = seller.Id,
            SellerAccount = seller,
            Make = "Honda",
            Model = "Civic",
            Year = 2018,
            Price = 18000.50m,
            MileageKm = 42000,
            Location = "Austin, TX",
            Description = "A commuter with service records.",
            Status = status,
            PublishedAt = publishedAt,
            UpdatedAt = publishedAt
        };
        if (photoId is not null && fileName is not null)
        {
            listing.Photos.Add(new ListingPhoto
            {
                Id = photoId,
                ListingId = id,
                Listing = listing,
                SortOrder = 0,
                ContentType = "image/png",
                StoredFileName = fileName,
                ByteSize = ListingRequests.Png.Length
            });
        }

        return listing;
    }

    private async Task DeleteSellerAsync(string sellerId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Inquiries.Where(inquiry => inquiry.Listing.SellerAccountId == sellerId).ExecuteDeleteAsync();
        await db.ListingPhotos.Where(photo => photo.Listing.SellerAccountId == sellerId).ExecuteDeleteAsync();
        await db.Listings.Where(listing => listing.SellerAccountId == sellerId).ExecuteDeleteAsync();
        await db.SellerAccounts.Where(account => account.Id == sellerId).ExecuteDeleteAsync();
    }
}
