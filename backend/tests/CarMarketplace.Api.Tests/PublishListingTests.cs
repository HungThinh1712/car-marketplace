using System.Net;
using CarMarketplace.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarMarketplace.Api.Tests;

[TestClass]
public sealed class PublishListingTests
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
    public async Task RegisterAndPublish_ValidListing_AppearsInTheAvailableList()
    {
        var email = $"publish-{Guid.NewGuid():N}@example.com";
        try
        {
            var register = await ListingRequests.RegisterAsync(_client, email);
            Assert.AreEqual(HttpStatusCode.Created, register.StatusCode);

            var publish = await _client.PostAsync("/api/v1/listings", ListingRequests.PublishForm());
            Assert.AreEqual(HttpStatusCode.Created, publish.StatusCode);
            using var created = await ListingRequests.ReadAsync(publish);
            var id = created.RootElement.GetProperty("id").GetString();
            Assert.IsNotNull(id);

            var list = await _client.GetAsync("/api/v1/listings");
            Assert.AreEqual(HttpStatusCode.OK, list.StatusCode);
            using var document = await ListingRequests.ReadAsync(list);
            var ids = document.RootElement.GetProperty("items").EnumerateArray()
                .Select(item => item.GetProperty("id").GetString())
                .ToList();
            Assert.Contains(id, ids);
        }
        finally
        {
            await DeleteByEmailAsync(email);
        }
    }

    [TestMethod]
    public async Task Register_SameEmailDifferentCase_ReturnsConflict()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        try
        {
            var first = await ListingRequests.RegisterAsync(_client, email);
            Assert.AreEqual(HttpStatusCode.Created, first.StatusCode);

            var second = await ListingRequests.RegisterAsync(_client, email.ToUpperInvariant());
            Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode);
            Assert.AreEqual("That email is already in use.", await ListingRequests.ReadDetailAsync(second));
        }
        finally
        {
            await DeleteByEmailAsync(email);
        }
    }

    [TestMethod]
    public async Task SignIn_WrongPassword_ReturnsUnauthorized()
    {
        var email = $"signin-{Guid.NewGuid():N}@example.com";
        try
        {
            var register = await ListingRequests.RegisterAsync(_client, email);
            Assert.AreEqual(HttpStatusCode.Created, register.StatusCode);

            var signIn = await _client.PostAsync(
                "/api/v1/sessions",
                ListingRequests.Json(new { email, password = "wrong-password" }));
            Assert.AreEqual(HttpStatusCode.Unauthorized, signIn.StatusCode);
            Assert.AreEqual("The email or password is incorrect.", await ListingRequests.ReadDetailAsync(signIn));
        }
        finally
        {
            await DeleteByEmailAsync(email);
        }
    }

    private async Task DeleteByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var sellerIds = await db.SellerAccounts
            .Where(account => account.Email == normalized)
            .Select(account => account.Id)
            .ToListAsync();
        await db.Inquiries.Where(inquiry => sellerIds.Contains(inquiry.Listing.SellerAccountId)).ExecuteDeleteAsync();
        await db.ListingPhotos.Where(photo => sellerIds.Contains(photo.Listing.SellerAccountId)).ExecuteDeleteAsync();
        await db.Listings.Where(listing => sellerIds.Contains(listing.SellerAccountId)).ExecuteDeleteAsync();
        await db.SellerAccounts.Where(account => sellerIds.Contains(account.Id)).ExecuteDeleteAsync();
    }
}
