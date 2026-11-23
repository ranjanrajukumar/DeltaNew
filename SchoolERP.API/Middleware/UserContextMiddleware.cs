using SchoolERP.API.Extensions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Authentication;

namespace SchoolERP.API.Middleware;

/// <summary>
/// Token + claims → the current user. Runs after UseAuthentication (JwtBearer has already checked the token's signature,
/// issuer, audience and expiry and put its claims on HttpContext.User). For every /api address outside the open areas it
/// loads the user named by the "id" claim and fills <see cref="Application.Common.Interfaces.ICurrentUser"/>, or answers
/// 401/403 with the Node app's { "error": ... } body (see <see cref="RequestAuthenticator"/>).
/// </summary>
public sealed class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IRequestAuthenticator authenticator)
    {
        var path = context.Request.ApiPath();
        if (!path.StartsWith("/api/", StringComparison.Ordinal) || ApiRequestExtensions.OpenAreas.Contains(ApiRequestExtensions.ApiArea(path)))
        {
            await next(context);
            return;
        }

        var refused = await authenticator.AuthenticateAsync(
            context.User.UserId(), context.Request.Method, context.Request.ApiUrl(), context.RequestAborted);
        if (refused is { } r)
        {
            context.Response.StatusCode = r.Status;
            await context.Response.WriteAsJsonAsync(r.Body, JsonDefaults.Options);
            return;
        }

        await next(context);
    }
}
