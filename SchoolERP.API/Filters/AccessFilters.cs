using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Common.Interfaces;
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
            var refused = await services.GetRequiredService<RequestAuthenticator>().AuthenticateAsync(
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

/// <summary>The Node app's requireRole(...) on one endpoint: only these base roles; anyone else gets 403 Forbidden.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireRoleAttribute(params string[] roles) : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        if (!user.IsInRole(roles)) context.Result = new ObjectResult(new { error = "Forbidden" }) { StatusCode = 403 };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
