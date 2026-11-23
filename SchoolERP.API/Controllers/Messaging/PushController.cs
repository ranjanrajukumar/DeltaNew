using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Application.Modules.Messaging;

namespace SchoolERP.API.Controllers;

/// <summary>
/// /api/push: every signed-in user (parents and students too) switches notifications on for their own device; sending is for
/// built-in admins only (the Node app's push/push.routes.js).
/// </summary>
[ApiController]
[Route("api/push")]
public sealed class PushController(IPushService push, ICurrentUser user) : ControllerBase
{
    private void AdminOnly()
    {
        if (user.Role != "admin" || user.CustomRole is not null) throw HttpException.Forbidden("Only an administrator can send notifications");
    }

    [HttpGet("key")]
    public async Task<IActionResult> Key(CancellationToken ct) => Ok(new { publicKey = await push.PublicKeyAsync(ct) });

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(new { devices = await push.MySubscriptionsAsync(user.Id, ct) });

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await push.SubscribeAsync(user.Id, json, Request.Headers.UserAgent.ToString(), ct);
        return StatusCode(201, new { ok = true });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        await push.UnsubscribeAsync(user.Id, new Body(json).Str("endpoint"), ct);
        return Ok(new { ok = true });
    }

    [HttpGet("audience")]
    public async Task<IActionResult> Audience([FromQuery] string? audience, CancellationToken ct)
    {
        AdminOnly();
        return Ok(await push.AudienceSizeAsync(audience, ct));
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] JsonObject? json, CancellationToken ct)
    {
        AdminOnly();
        return Ok(await push.SendAsync(json, ct));
    }
}
