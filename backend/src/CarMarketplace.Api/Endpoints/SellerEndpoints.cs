using System.Security.Claims;
using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Middleware;
using CarMarketplace.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarMarketplace.Api.Endpoints;

/// <summary>Seller registration and the current account.</summary>
public static class SellerEndpoints
{
    /// <summary>Maps seller account routes.</summary>
    public static void MapSellerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/sellers").WithTags("Sellers");

        group.MapPost("/", RegisterAsync)
            .RequireRateLimiting("abuse")
            .WithName("RegisterSeller")
            .WithSummary("Create a seller account")
            .WithDescription("Creates an account, signs the caller in, and returns the seller.")
            .Produces<SellerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/me", GetCurrentAsync)
            .WithName("GetCurrentSeller")
            .WithSummary("Get the signed-in seller")
            .WithDescription("Returns the caller's id, display name, and email.")
            .Produces<SellerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me/listings", ListMineAsync)
            .WithName("ListMyListings")
            .WithSummary("List the signed-in seller's cars")
            .WithDescription("Returns every status for the caller, 20 per page, newest published first.")
            .Produces<ListingListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Results<Created<SellerResponse>, ProblemHttpResult>> RegisterAsync(
        RegisterSellerRequest? request,
        ISellerAccountService sellers,
        CancellationToken cancellationToken)
    {
        var seller = await sellers.RegisterAsync(request ?? new RegisterSellerRequest(), cancellationToken);
        return TypedResults.Created("/api/v1/sellers/me", seller);
    }

    private static async Task<Results<Ok<SellerResponse>, ProblemHttpResult>> GetCurrentAsync(
        ClaimsPrincipal user,
        ISellerAccountService sellers,
        CancellationToken cancellationToken)
    {
        var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        var seller = await sellers.GetCurrentAsync(sellerId, cancellationToken);
        return seller is null ? ApiProblems.Unauthorized() : TypedResults.Ok(seller);
    }

    private static async Task<Results<Ok<ListingListResponse>, ProblemHttpResult>> ListMineAsync(
        ClaimsPrincipal user,
        int? page,
        IListingCommandService listings,
        CancellationToken cancellationToken)
    {
        var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        var pageNumber = page ?? 1;
        if (pageNumber < 1)
        {
            return ApiProblems.BadPage();
        }

        var result = await listings.ListMineAsync(sellerId, pageNumber, cancellationToken);
        return TypedResults.Ok(result);
    }
}
