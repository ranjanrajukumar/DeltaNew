using Microsoft.AspNetCore.Http;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.API.Middleware;

/// <summary>
/// Every failure becomes { "error": "..." } with the status the Node app used (middleware/errorMiddleware.js):
/// HttpException keeps its status (plus "errors" for validation lists), database problems are translated without exposing
/// table or column names, and anything else is a logged 500 "Server error".
/// </summary>
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await WriteAsync(context, ex);
        }
    }

    private async Task WriteAsync(HttpContext context, Exception ex)
    {
        switch (ex)
        {
            case HttpException http:
            {
                var body = new Dictionary<string, object?> { ["error"] = http.Message };
                if (http.Errors is not null) body["errors"] = http.Errors;
                if (http.Extra is not null) foreach (var (k, v) in http.Extra) body[k] = v;
                await Respond(context, http.Status, body);
                return;
            }
            case BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge }:
                await Respond(context, 413, new { error = "Request body is too large for this endpoint" });
                return;
            case OperationCanceledException when context.RequestAborted.IsCancellationRequested:
                return;
        }

        var translator = context.RequestServices.GetService<DbErrorTranslator>();
        if (translator?.Translate(ex) is { } db)
        {
            logger.LogWarning("Database error answered with {Status}: {Detail}", db.Status, db.LogDetail);
            await Respond(context, db.Status, new { error = db.Message });
            return;
        }

        logger.LogError(ex, "Unhandled error on {Method} {Path}", context.Request.Method, context.Request.Path);
        await Respond(context, 500, new { error = "Server error" });
    }

    private static Task Respond(HttpContext context, int status, object body)
    {
        context.Response.Clear();
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(body, SchoolERP.Application.Common.Helpers.JsonDefaults.Options);
    }
}
