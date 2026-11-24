using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CarMarketplace.Api.Middleware;

/// <summary>Shared RFC 7807 responses.</summary>
public static class ApiProblems
{
    /// <summary>401 returned when a seller session is required.</summary>
    public static ProblemHttpResult Unauthorized() =>
        Problem(StatusCodes.Status401Unauthorized, "Unauthorized", "Sign in to continue.");

    /// <summary>400 returned for a page number below 1.</summary>
    public static ProblemHttpResult BadPage() =>
        Problem(StatusCodes.Status400BadRequest, "Bad Request", "Page must be a whole number of 1 or greater.");

    /// <summary>400 returned for a status query other than available, sold, or all.</summary>
    public static ProblemHttpResult BadStatusFilter() =>
        Problem(StatusCodes.Status400BadRequest, "Bad Request", "Status must be available, sold, or all.");

    /// <summary>404 returned when a listing id does not exist.</summary>
    public static ProblemHttpResult ListingNotFound() =>
        Problem(StatusCodes.Status404NotFound, "Not Found", "That listing was not found.");

    /// <summary>Creates a problem result.</summary>
    public static ProblemHttpResult Problem(int status, string title, string detail) =>
        TypedResults.Problem(detail: detail, statusCode: status, title: title);

    /// <summary>Writes a problem response.</summary>
    public static Task WriteAsync(
        HttpResponse response,
        HttpRequest request,
        int status,
        string title,
        string detail,
        CancellationToken cancellationToken = default)
    {
        response.StatusCode = status;
        return response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = request.Path
        }, cancellationToken);
    }
}
