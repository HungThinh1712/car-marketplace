using CarMarketplace.Api.Middleware;
using CarMarketplace.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CarMarketplace.Api.Endpoints;

/// <summary>Public photo bytes.</summary>
public static class PhotoEndpoints
{
    /// <summary>Maps photo routes.</summary>
    public static void MapPhotoEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/photos/{id}", GetAsync)
            .WithTags("Photos")
            .WithName("GetPhoto")
            .WithSummary("Get a listing photo")
            .WithDescription("Returns the re-encoded image bytes and content type.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<FileStreamHttpResult, ProblemHttpResult>> GetAsync(
        string id,
        IListingBrowseService listings,
        CancellationToken cancellationToken)
    {
        var photo = await listings.OpenPhotoAsync(id, cancellationToken);
        if (photo is null)
        {
            return ApiProblems.Problem(StatusCodes.Status404NotFound, "Not Found", "That photo was not found.");
        }

        return TypedResults.File(photo.Stream, photo.ContentType);
    }
}
