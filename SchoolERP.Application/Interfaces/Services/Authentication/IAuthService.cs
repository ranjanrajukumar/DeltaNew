using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Signing in and the signed-in user's own account (the Node app's platform/auth.service.js).</summary>
public interface IAuthService
{
    /// <summary>{ token, user: { id, name, role }, mustChangePassword } for valid credentials, or null.</summary>
    Task<(object Session, User User)?> LoginAsync(string? identifier, string? password, CancellationToken ct = default);

    /// <summary>The sign-in answer for a user whose identity is established.</summary>
    object SessionFor(User user);

    /// <summary>Records a sign-in attempt. It must never break signing in, so failures are swallowed.</summary>
    Task RecordLoginAsync(string? ip, string? userAgent, string? email, User? user, bool success, CancellationToken ct = default);

    /// <summary>The signed-in user's current access.</summary>
    Task<object> MeAsync(CancellationToken ct = default);

    Task ChangePasswordAsync(string? currentPassword, JsonNode? newPassword, CancellationToken ct = default);
}
