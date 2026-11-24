using System.Globalization;
using System.Security.Claims;
using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Middleware;
using CarMarketplace.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarMarketplace.Api.Endpoints;

/// <summary>Public catalog and owner listing writes.</summary>
public static class ListingEndpoints
{
    /// <summary>Maps listing routes.</summary>
    public static void MapListingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/listings").WithTags("Listings");

        group.MapGet("/", ListAsync)
            .WithName("ListListings")
            .WithSummary("Browse cars")
            .WithDescription("Returns 20 listings per page. Status defaults to available. A page past the end is empty and keeps the real total count.")
            .Produces<ListingListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id}", GetAsync)
            .WithName("GetListing")
            .WithSummary("Get one car")
            .WithDescription("Returns an available or sold listing. Unknown ids return 404.")
            .Produces<ListingResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", PublishAsync)
            .DisableAntiforgery()
            .WithName("PublishListing")
            .WithSummary("Publish a car")
            .WithDescription("Accepts multipart facts and 1 to 10 photos. The stored status is Available.")
            .Produces<ListingResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id}", ReplaceFactsAsync)
            .WithName("ReplaceListingFacts")
            .WithSummary("Replace listing facts")
            .WithDescription("Replaces make, model, year, price, mileage, location, and description. Photos, status, and the original publication time stay unchanged.")
            .Produces<ListingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id}/photos", ReplacePhotosAsync)
            .DisableAntiforgery()
            .WithName("ReplaceListingPhotos")
            .WithSummary("Replace listing photos")
            .WithDescription("Replaces the whole photo set. The first file is the cover. An invalid set keeps the previous photos.")
            .Produces<ListingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{id}/status", ChangeStatusAsync)
            .WithName("ChangeListingStatus")
            .WithSummary("Mark a car sold or available")
            .WithDescription("The owner can move a listing between Available and Sold. The original publication time does not change.")
            .Produces<ListingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<ListingListResponse>, ProblemHttpResult>> ListAsync(
        string? make,
        string? model,
        string? location,
        int? minYear,
        int? maxYear,
        decimal? minPrice,
        decimal? maxPrice,
        int? maxMileage,
        string? status,
        int? page,
        IListingBrowseService listings,
        CancellationToken cancellationToken)
    {
        var pageNumber = page ?? 1;
        if (pageNumber < 1)
        {
            return ApiProblems.BadPage();
        }

        if (!TryParseStatus(status, out var statusFilter))
        {
            return ApiProblems.BadStatusFilter();
        }

        var result = await listings.SearchAsync(
            make,
            model,
            location,
            minYear,
            maxYear,
            minPrice,
            maxPrice,
            maxMileage,
            statusFilter,
            pageNumber,
            cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ListingResponse>, ProblemHttpResult>> GetAsync(
        string id,
        IListingBrowseService listings,
        CancellationToken cancellationToken)
    {
        var listing = await listings.GetAsync(id, cancellationToken);
        return listing is null ? ApiProblems.ListingNotFound() : TypedResults.Ok(listing);
    }

    private static async Task<Results<Created<ListingResponse>, ProblemHttpResult>> PublishAsync(
        HttpRequest request,
        ClaimsPrincipal user,
        IListingCommandService listings,
        CancellationToken cancellationToken)
    {
        var sellerId = RequireSeller(user);
        if (sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        if (!request.HasFormContentType)
        {
            return ApiProblems.Problem(StatusCodes.Status400BadRequest, "Bad Request", "The listing was not saved.");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var listing = await listings.PublishAsync(sellerId, ReadFacts(form), form.Files.GetFiles("photos"), cancellationToken);
        return TypedResults.Created($"/api/v1/listings/{listing.Id}", listing);
    }

    private static async Task<Results<Ok<ListingResponse>, ProblemHttpResult>> ReplaceFactsAsync(
        string id,
        ReplaceListingRequest? request,
        ClaimsPrincipal user,
        IListingCommandService listings,
        CancellationToken cancellationToken)
    {
        var sellerId = RequireSeller(user);
        if (sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        var body = request ?? new ReplaceListingRequest();
        var listing = await listings.ReplaceFactsAsync(
            sellerId,
            id,
            new ListingFacts(body.Make, body.Model, body.Year, body.Price, body.Mileage, body.Location, body.Description),
            cancellationToken);
        return TypedResults.Ok(listing);
    }

    private static async Task<Results<Ok<ListingResponse>, ProblemHttpResult>> ReplacePhotosAsync(
        string id,
        HttpRequest request,
        ClaimsPrincipal user,
        IListingCommandService listings,
        CancellationToken cancellationToken)
    {
        var sellerId = RequireSeller(user);
        if (sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        if (!request.HasFormContentType)
        {
            return ApiProblems.Problem(StatusCodes.Status400BadRequest, "Bad Request", "The listing was not saved.");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var listing = await listings.ReplacePhotosAsync(sellerId, id, form.Files.GetFiles("photos"), cancellationToken);
        return TypedResults.Ok(listing);
    }

    private static async Task<Results<Ok<ListingResponse>, ProblemHttpResult>> ChangeStatusAsync(
        string id,
        ChangeListingStatusRequest? request,
        ClaimsPrincipal user,
        IListingCommandService listings,
        CancellationToken cancellationToken)
    {
        var sellerId = RequireSeller(user);
        if (sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        var listing = await listings.ChangeStatusAsync(sellerId, id, request?.Status, cancellationToken);
        return TypedResults.Ok(listing);
    }

    private static string? RequireSeller(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.NameIdentifier) : null;

    private static bool TryParseStatus(string? status, out ListingStatusFilter filter)
    {
        if (string.IsNullOrWhiteSpace(status) || status.Equals("available", StringComparison.OrdinalIgnoreCase))
        {
            filter = ListingStatusFilter.Available;
            return true;
        }

        if (status.Equals("sold", StringComparison.OrdinalIgnoreCase))
        {
            filter = ListingStatusFilter.Sold;
            return true;
        }

        if (status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filter = ListingStatusFilter.All;
            return true;
        }

        filter = default;
        return false;
    }

    private static ListingFacts ReadFacts(IFormCollection form) =>
        new(
            form["make"],
            form["model"],
            ParseInt(form["year"]),
            ParseDecimal(form["price"]),
            ParseInt(form["mileage"]),
            form["location"],
            form["description"]);

    private static int? ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static decimal? ParseDecimal(string? value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
}
