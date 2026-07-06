using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Nemeris.Api.Middleware;

/// <summary>
/// Last-resort exception boundary. Business-rule violations (InvalidOperationException
/// carrying an "Error.*" resource key from Infrastructure) become localized 400s;
/// anything else becomes a generic localized 500 — internals never leak to clients.
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IStringLocalizer<SharedResources> localizer)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex) when (!context.Response.HasStarted)
        {
            logger.LogWarning("Business rule rejected request {Path}: {Key}", context.Request.Path, ex.Message);
            // The exception message is a resource key; unknown keys fall through as-is.
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, localizer[ex.Message]);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, localizer["Error.Unexpected"]);
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = statusCode, Title = title },
            options: null,
            contentType: "application/problem+json");
    }
}
