using CarMarketplace.Api.Contracts;
using CarMarketplace.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarMarketplace.Api.Endpoints;

/// <summary>Sign-in and sign-out.</summary>
public static class SessionEndpoints
{
    /// <summary>Maps session routes.</summary>
    public static void MapSessionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/sessions").WithTags("Sessions");

        group.MapPost("/", SignInAsync)
            .RequireRateLimiting("abuse")
            .WithName("SignIn")
            .WithSummary("Sign in")
            .WithDescription("Issues a session cookie that expires 14 days after this sign-in. Later activity does not extend it.")
            .Produces<SellerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapDelete("/current", SignOutAsync)
            .WithName("SignOut")
            .WithSummary("Sign out")
            .WithDescription("Clears the session cookie whether or not a session was present.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<SellerResponse>, ProblemHttpResult>> SignInAsync(
        SignInRequest? request,
        ISellerAccountService sellers,
        CancellationToken cancellationToken)
    {
        var seller = await sellers.SignInAsync(request ?? new SignInRequest(), cancellationToken);
        return TypedResults.Ok(seller);
    }

    private static async Task<NoContent> SignOutAsync(
        ISellerAccountService sellers,
        CancellationToken cancellationToken)
    {
        await sellers.SignOutAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}
