using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Authentication;

/// <summary>
/// The Node app's requireAuth: given the user id from an already-validated token (JwtBearer), loads the user so a deleted
/// account or a changed role takes effect immediately. A temporary password must be changed first; a custom role is checked
/// against its modules; then role rights.
/// </summary>
public sealed class RequestAuthenticator(
    IUserRepository users,
    ICurrentUserSetter setter,
    IRightsService rights) : IRequestAuthenticator
{
    /// <summary>Null when the request may go on; otherwise the status and JSON body to answer with.</summary>
    public async Task<(int Status, object Body)?> AuthenticateAsync(int? userId, string method, string originalUrl, CancellationToken ct = default)
    {
        var user = userId is null ? null : await users.GetWithRoleAsync(userId.Value, ct);
        if (user is null) return (401, new { error = "Unauthorized" });

        if (user.MustChangePassword && !originalUrl.StartsWith("/api/auth/", StringComparison.Ordinal))
            return (403, new { error = "Please change your password first.", mustChangePassword = true });

        var role = user.RoleNavigation?.BaseRole ?? user.Role;
        string? customRole = null;
        JsonObject? permissions = null;
        if (user.RoleId is not null)
        {
            customRole = user.RoleNavigation?.Name;
            permissions = new JsonObject(); // a role that vanished grants nothing
            if (user.RoleNavigation is { } r)
            {
                try { permissions = JsonNode.Parse(r.Permissions) as JsonObject ?? new JsonObject(); }
                catch (System.Text.Json.JsonException) { }
            }
        }
        setter.Set(user.Id, user.Name, role, user.StaffId, user.MustChangePassword, customRole, permissions);

        if (permissions is not null && !AccessRules.Permits(permissions, method, originalUrl))
            return (403, new { error = "Your role does not include this area" });
        if (!await rights.AllowsAsync(role, method, originalUrl, ct))
            return (403, new { error = "Your role is not allowed to do this here" });
        return null;
    }
}
