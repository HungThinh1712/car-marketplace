namespace CarMarketplace.Api.Middleware;

/// <summary>Rejects mutating browser requests whose Origin is not the configured site.</summary>
public sealed class OriginCheckMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    /// <summary>Allows requests with no Origin. A present Origin must match <c>WebOrigin</c>.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (MutatingMethods.Contains(context.Request.Method)
            && context.Request.Headers.TryGetValue("Origin", out var originValues))
        {
            var origin = originValues.ToString();
            if (!string.IsNullOrWhiteSpace(origin))
            {
                var allowed = configuration["WebOrigin"];
                if (!string.Equals(origin, allowed, StringComparison.Ordinal))
                {
                    await ApiProblems.WriteAsync(
                        context.Response,
                        context.Request,
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "This action is not allowed from that site.",
                        context.RequestAborted);
                    return;
                }
            }
        }

        await next(context);
    }
}
