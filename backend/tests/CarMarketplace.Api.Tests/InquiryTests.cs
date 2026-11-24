using System.Net;
using CarMarketplace.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarMarketplace.Api.Tests;

[TestClass]
public sealed class InquiryTests
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
    public async Task AnonymousInquiry_AvailableCar_OwnerCanReadItAndSoldCarRejectsAnother()
    {
        var ownerEmail = $"inquiry-owner-{Guid.NewGuid():N}@example.com";
        try
        {
            Assert.AreEqual(HttpStatusCode.Created, (await ListingRequests.RegisterAsync(_owner, ownerEmail)).StatusCode);
            var publish = await _owner.PostAsync("/api/v1/listings", ListingRequests.PublishForm());
            Assert.AreEqual(HttpStatusCode.Created, publish.StatusCode);
            using var created = await ListingRequests.ReadAsync(publish);
            var id = created.RootElement.GetProperty("id").GetString();
            Assert.IsNotNull(id);

            using var buyer = ListingRequests.CreateClient(_factory);
            var inquiry = await buyer.PostAsync(
                $"/api/v1/listings/{id}/inquiries",
                ListingRequests.Json(new
                {
                    name = "Amina Cole",
                    email = "amina@example.com",
                    phone = "+1 (415) 555-0134",
                    message = "Is the mileage in kilometers, and is the car still available?"
                }));
            Assert.AreEqual(HttpStatusCode.Created, inquiry.StatusCode);
            using var confirmation = await ListingRequests.ReadAsync(inquiry);
            Assert.AreEqual("Honda", confirmation.RootElement.GetProperty("make").GetString());
            Assert.AreEqual("Civic", confirmation.RootElement.GetProperty("model").GetString());
            Assert.AreEqual(2018, confirmation.RootElement.GetProperty("year").GetInt32());

            var list = await _owner.GetAsync($"/api/v1/listings/{id}/inquiries");
            Assert.AreEqual(HttpStatusCode.OK, list.StatusCode);
            using var inquiries = await ListingRequests.ReadAsync(list);
            var items = inquiries.RootElement.GetProperty("items");
            Assert.HasCount(1, items.EnumerateArray().ToList());
            var row = items[0];
            Assert.AreEqual("Amina Cole", row.GetProperty("name").GetString());
            Assert.AreEqual("amina@example.com", row.GetProperty("email").GetString());
            Assert.AreEqual("Is the mileage in kilometers, and is the car still available?", row.GetProperty("message").GetString());
            Assert.AreEqual("+1 (415) 555-0134", row.GetProperty("phone").GetString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(row.GetProperty("submittedAt").GetString()));

            var sold = await _owner.PatchAsync(
                $"/api/v1/listings/{id}/status",
                ListingRequests.Json(new { status = "Sold" }));
            Assert.AreEqual(HttpStatusCode.OK, sold.StatusCode);

            var rejected = await buyer.PostAsync(
                $"/api/v1/listings/{id}/inquiries",
                ListingRequests.Json(new
                {
                    name = "Amina Cole",
                    email = "amina@example.com",
                    message = "Is the car still available for a visit tomorrow?"
                }));
            Assert.AreEqual(HttpStatusCode.Conflict, rejected.StatusCode);
            Assert.AreEqual("This car is sold and is not accepting inquiries.", await ListingRequests.ReadDetailAsync(rejected));

            var after = await _owner.GetAsync($"/api/v1/listings/{id}/inquiries");
            using var afterDocument = await ListingRequests.ReadAsync(after);
            Assert.HasCount(1, afterDocument.RootElement.GetProperty("items").EnumerateArray().ToList());
        }
        finally
        {
            await DeleteByEmailAsync(ownerEmail);
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
