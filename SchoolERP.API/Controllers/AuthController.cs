using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Filters;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Authentication;

namespace SchoolERP.API.Controllers;

/// <summary>/api/health and /api/auth (the Node app's routes/index.js health check and platform/auth.routes.js).</summary>
[ApiController]
[Route("api")]
public sealed class AuthController(AuthService auth, LoginLimiter limiter) : ControllerBase
{
    /// <summary>Is the API up.</summary>
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { ok = true });

    /// <summary>Sign in with an email (or a student's admission number) and password.</summary>
    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = new Body(json);
        var email = body.Str("email");
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var wait = limiter.SecondsToWait(ip, email);
        if (wait > 0)
        {
            Response.Headers.RetryAfter = wait.ToString();
            return StatusCode(429, new { error = $"Too many failed attempts. Try again in {(int)Math.Ceiling(wait / 60.0)} minute(s)." });
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        var password = body.Raw("password") is JsonValue v && v.TryGetValue<string>(out var p) ? p : null;
        var result = await auth.LoginAsync(email, password, ct);
        if (result is null)
        {
            limiter.RecordFailure(ip, email);
            await auth.RecordLoginAsync(ip, userAgent, email, null, false, ct);
            return Unauthorized(new { error = "Invalid credentials" });
        }
        limiter.RecordSuccess(ip, email);
        await auth.RecordLoginAsync(ip, userAgent, email, result.Value.User, true, ct);
        return Ok(result.Value.Session);
    }

    /// <summary>The signed-in user's own profile and access, so the UI can hide what a custom role does not include.</summary>
    [HttpGet("auth/me")]
    [RequireAuth]
    public async Task<IActionResult> Me(CancellationToken ct) => Ok(await auth.MeAsync(ct));

    /// <summary>{ currentPassword, newPassword }: anyone signed in, and the only thing a temporary password may do.</summary>
    [HttpPost("auth/change-password")]
    [RequireAuth]
    public async Task<IActionResult> ChangePassword([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        var body = new Body(json);
        await auth.ChangePasswordAsync(body.Str("currentPassword"), body.Raw("newPassword"), ct);
        return Ok(new { ok = true });
    }
}
