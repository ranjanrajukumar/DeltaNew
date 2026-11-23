using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Authentication;

namespace SchoolERP.API.Filters;

/// <summary>
/// The Node app's requireAuth on a single endpoint inside an open area (for example /api/auth/me): signs the user in from the
/// token, or answers 401/403 the same way the area-wide check does.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        if (!services.GetRequiredService<ICurrentUser>().IsAuthenticated)
        {
            var http = context.HttpContext;
            var refused = await services.GetRequiredService<IRequestAuthenticator>().AuthenticateAsync(
                http.User.UserId(), http.Request.Method, http.Request.ApiUrl(), http.RequestAborted);
            if (refused is { } r)
            {
                context.Result = new ObjectResult(r.Body) { StatusCode = r.Status };
                return;
            }
        }
        await next();
    }
}
