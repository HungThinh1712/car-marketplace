using System.Security.Claims;
using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Middleware;
using CarMarketplace.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarMarketplace.Api.Endpoints;

/// <summary>Buyer inquiries and the owner's read-only list.</summary>
public static class InquiryEndpoints
{
    /// <summary>Maps inquiry routes. There is no reply route.</summary>
    public static void MapInquiryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/listings/{id}/inquiries").WithTags("Inquiries");

        group.MapPost("/", CreateAsync)
            .RequireRateLimiting("abuse")
            .WithName("CreateInquiry")
            .WithSummary("Send an inquiry")
            .WithDescription("Stores one message about someone else's available car. A buyer may send more than one. There is no reply.")
            .Produces<InquiryCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/", ListAsync)
            .WithName("ListInquiries")
            .WithSummary("Read inquiries")
            .WithDescription("Returns the owner's inquiries, newest first. There is no reply.")
            .Produces<InquiryListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Created<InquiryCreatedResponse>, ProblemHttpResult>> CreateAsync(
        string id,
        CreateInquiryRequest? request,
        ClaimsPrincipal user,
        IInquiryService inquiries,
        CancellationToken cancellationToken)
    {
        var sellerId = user.Identity?.IsAuthenticated == true
            ? user.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var created = await inquiries.CreateAsync(sellerId, id, request ?? new CreateInquiryRequest(), cancellationToken);
        return TypedResults.Created($"/api/v1/listings/{created.ListingId}", created);
    }

    private static async Task<Results<Ok<InquiryListResponse>, ProblemHttpResult>> ListAsync(
        string id,
        ClaimsPrincipal user,
        IInquiryService inquiries,
        CancellationToken cancellationToken)
    {
        var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || sellerId is null)
        {
            return ApiProblems.Unauthorized();
        }

        var result = await inquiries.ListForOwnerAsync(sellerId, id, cancellationToken);
        return TypedResults.Ok(result);
    }
}
