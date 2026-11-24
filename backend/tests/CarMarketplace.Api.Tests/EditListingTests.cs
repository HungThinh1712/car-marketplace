using System.Net;
using System.Text.Json;
using CarMarketplace.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarMarketplace.Api.Tests;

[TestClass]
public sealed class EditListingTests
{
    private MarketplaceApiFactory _factory = null!;
    private HttpClient _owner = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new MarketplaceApiFactory();
        _owner = ListingRequests.CreateClient(_factory);
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await db.Database.MigrateAsync();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _owner.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task Owner_ChangesPriceAndMarksSold_OtherSellerIsForbidden()
    {
        var ownerEmail = $"owner-{Guid.NewGuid():N}@example.com";
        var otherEmail = $"other-{Guid.NewGuid():N}@example.com";
        try
        {
            Assert.AreEqual(HttpStatusCode.Created, (await ListingRequests.RegisterAsync(_owner, ownerEmail)).StatusCode);
            var publish = await _owner.PostAsync("/api/v1/listings", ListingRequests.PublishForm());
            Assert.AreEqual(HttpStatusCode.Created, publish.StatusCode);
            using var created = await ListingRequests.ReadAsync(publish);
            var id = created.RootElement.GetProperty("id").GetString();
            Assert.IsNotNull(id);
            var publishedAt = created.RootElement.GetProperty("publishedAt").GetString();

            var update = await _owner.PutAsync(
                $"/api/v1/listings/{id}",
                ListingRequests.Json(new
                {
                    make = "Honda",
                    model = "Civic",
                    year = 2018,
                    price = 12345.50,
                    mileage = 42000,
                    location = "Oakland, CA",
                    description = "A clean commuter with a full service history."
                }));
            Assert.AreEqual(HttpStatusCode.OK, update.StatusCode);
            using var updated = await ListingRequests.ReadAsync(update);
            Assert.AreEqual(publishedAt, updated.RootElement.GetProperty("publishedAt").GetString());
            Assert.AreEqual(12345.50m, updated.RootElement.GetProperty("price").GetDecimal());

            var sold = await _owner.PatchAsync(
                $"/api/v1/listings/{id}/status",
                ListingRequests.Json(new { status = "Sold" }));
            Assert.AreEqual(HttpStatusCode.OK, sold.StatusCode);

            var list = await _owner.GetAsync("/api/v1/listings");
            using var listDocument = await ListingRequests.ReadAsync(list);
            var ids = listDocument.RootElement.GetProperty("items").EnumerateArray()
                .Select(item => item.GetProperty("id").GetString())
                .ToList();
            Assert.DoesNotContain(id, ids);

            var detail = await _owner.GetAsync($"/api/v1/listings/{id}");
            using var detailDocument = await ListingRequests.ReadAsync(detail);
            Assert.AreEqual("Sold", detailDocument.RootElement.GetProperty("status").GetString());

            using var other = ListingRequests.CreateClient(_factory);
            Assert.AreEqual(HttpStatusCode.Created, (await ListingRequests.RegisterAsync(other, otherEmail, "Other Yard")).StatusCode);
            var forbidden = await other.PutAsync(
                $"/api/v1/listings/{id}",
                ListingRequests.Json(new
                {
                    make = "Honda",
                    model = "Civic",
                    year = 2018,
                    price = 1.00,
                    mileage = 42000,
                    location = "Oakland, CA",
                    description = "A clean commuter with a full service history."
                }));
            Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode);
            Assert.AreEqual("Only the owner can change this listing.", await ListingRequests.ReadDetailAsync(forbidden));

            var after = await _owner.GetAsync($"/api/v1/listings/{id}");
            using var afterDocument = await ListingRequests.ReadAsync(after);
            Assert.AreEqual(12345.50m, afterDocument.RootElement.GetProperty("price").GetDecimal());
        }
        finally
        {
            await DeleteByEmailAsync(ownerEmail);
            await DeleteByEmailAsync(otherEmail);
        }
    }

    private async Task DeleteByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var sellerIds = await db.SellerAccounts.Where(account => account.Email == normalized).Select(account => account.Id).ToListAsync();
        await db.Inquiries.Where(inquiry => sellerIds.Contains(inquiry.Listing.SellerAccountId)).ExecuteDeleteAsync();
        await db.ListingPhotos.Where(photo => sellerIds.Contains(photo.Listing.SellerAccountId)).ExecuteDeleteAsync();
        await db.Listings.Where(listing => sellerIds.Contains(listing.SellerAccountId)).ExecuteDeleteAsync();
        await db.SellerAccounts.Where(account => sellerIds.Contains(account.Id)).ExecuteDeleteAsync();
    }
}
