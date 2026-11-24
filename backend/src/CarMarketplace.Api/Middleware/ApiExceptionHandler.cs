using CarMarketplace.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarMarketplace.Api.Middleware;

/// <summary>Maps known marketplace failures to RFC 7807 problem details. A 500 response does not copy the exception message.</summary>
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>Writes a problem response for marketplace failures and unexpected exceptions.</summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case FieldValidationException validation:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new HttpValidationProblemDetails(new Dictionary<string, string[]>(validation.Errors))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Detail = validation.ClientDetail,
                    Instance = httpContext.Request.Path
                }, cancellationToken);
                return true;
            case NotFoundException notFound:
                await ApiProblems.WriteAsync(httpContext.Response, httpContext.Request, StatusCodes.Status404NotFound, "Not Found", notFound.ClientDetail, cancellationToken);
                return true;
            case ForbiddenException forbidden:
                await ApiProblems.WriteAsync(httpContext.Response, httpContext.Request, StatusCodes.Status403Forbidden, "Forbidden", forbidden.ClientDetail, cancellationToken);
                return true;
            case ConflictException conflict:
                await ApiProblems.WriteAsync(httpContext.Response, httpContext.Request, StatusCodes.Status409Conflict, "Conflict", conflict.ClientDetail, cancellationToken);
                return true;
            case UnauthorizedException unauthorized:
                await ApiProblems.WriteAsync(httpContext.Response, httpContext.Request, StatusCodes.Status401Unauthorized, "Unauthorized", unauthorized.ClientDetail, cancellationToken);
                return true;
            default:
                logger.LogError(
                    "Unhandled {ExceptionType} at {Path}. {StackTrace}",
                    exception.GetType().FullName,
                    httpContext.Request.Path.Value,
                    exception.StackTrace);
                await ApiProblems.WriteAsync(
                    httpContext.Response,
                    httpContext.Request,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    "The request could not be completed.",
                    cancellationToken);
                return true;
        }
    }
}
