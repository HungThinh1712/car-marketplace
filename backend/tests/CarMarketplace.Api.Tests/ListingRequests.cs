using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CarMarketplace.Api.Tests;

internal static class ListingRequests
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    public static StringContent Json(object body)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static MultipartFormDataContent PublishForm()
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Honda"), "make");
        content.Add(new StringContent("Civic"), "model");
        content.Add(new StringContent("2018"), "year");
        content.Add(new StringContent("15000.50"), "price");
        content.Add(new StringContent("42000"), "mileage");
        content.Add(new StringContent("Oakland, CA"), "location");
        content.Add(new StringContent("A clean commuter with a full service history."), "description");
        var file = new ByteArrayContent(Png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "photos", "civic.png");
        return content;
    }

    public static HttpClient CreateClient(MarketplaceApiFactory factory) =>
        factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

    public static async Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string displayName = "North Yard Motors") =>
        await client.PostAsync(
            "/api/v1/sellers",
            Json(new { displayName, email, password = "test-pass-8" }));

    public static async Task<string> ReadDetailAsync(HttpResponseMessage response)
    {
        var document = await ReadAsync(response);
        return document.RootElement.TryGetProperty("detail", out var detail) ? detail.GetString() ?? "" : "";
    }

    public static async Task<JsonDocument> ReadAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
