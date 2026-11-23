using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Authentication;

namespace SchoolERP.API.Filters;

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
