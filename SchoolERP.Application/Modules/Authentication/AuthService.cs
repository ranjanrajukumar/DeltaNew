using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Authentication;

/// <summary>Signing in and the signed-in user's own account (the Node app's platform/auth.routes.js, auth.service.js and loginActivity.js).</summary>
public sealed class AuthService(
    IUserRepository users,
    ILoginEventRepository loginEvents,
    IUnitOfWork unitOfWork,
    IPasswordHasher hasher,
    ITokenService tokens,
    ICurrentUser currentUser,
    IClassService classes) : IAuthService
{
    private const int RetentionDays = 180;

    // Compared against when the account is unknown, so a missing account takes as long as a wrong password.
    private static string? _dummyHash;
    private string DummyHash => _dummyHash ??= hasher.Hash("not-a-real-password");

    /// <summary>{ token, user: { id, name, role }, mustChangePassword } for valid credentials, or null.</summary>
    public async Task<(object Session, User User)?> LoginAsync(string? identifier, string? password, CancellationToken ct = default)
    {
        // an email signs in as itself; anything else is a student's admission number
        var who = identifier?.Trim() ?? "";
        User? user = null;
        if (who != "")
            user = who.Contains('@')
                ? await users.GetByEmailAsync(who, ct)
                : await users.GetByAdmissionNoAsync(who, ct);
        var valid = hasher.Verify(password ?? "", user?.PasswordHash ?? DummyHash);
        if (user is null || !valid) return null;
        return (SessionFor(user), user);
    }

    /// <summary>The sign-in answer for a user whose identity is established: a token, who they are, and whether a new password is due.</summary>
    public object SessionFor(User user) => new
    {
        token = tokens.CreateToken(user.Id, user.Name, user.Role),
        user = new { id = user.Id, name = user.Name, role = user.Role },
        mustChangePassword = user.MustChangePassword,
    };

    /// <summary>Records a sign-in attempt. It must never break signing in, so failures are swallowed.</summary>
    public async Task RecordLoginAsync(string? ip, string? userAgent, string? email, User? user, bool success, CancellationToken ct = default)
    {
        try
        {
            var emailText = email ?? "";
            var account = user;
            if (account is null && !success)
                account = await users.GetByEmailAsync(emailText, ct);
            loginEvents.Add(new LoginEvent
            {
                UserId = account?.Id,
                Email = emailText.Length > 150 ? emailText[..150] : emailText,
                Role = account?.Role,
                Success = success,
                Ip = string.IsNullOrEmpty(ip) ? null : ip.Length > 64 ? ip[..64] : ip,
                UserAgent = string.IsNullOrEmpty(userAgent) ? null : userAgent.Length > 200 ? userAgent[..200] : userAgent,
            });
            await unitOfWork.SaveChangesAsync(ct);
            if (success)
            {
                var cutoff = DateTime.UtcNow.AddDays(-RetentionDays);
                await loginEvents.DeleteOlderThanAsync(cutoff, ct);
            }
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            // login history is best effort
        }
    }

    /// <summary>The signed-in user's current access, so the UI can hide what a custom role does not include.</summary>
    public async Task<object> MeAsync(CancellationToken ct = default) => new
    {
        id = currentUser.Id,
        name = currentUser.Name,
        role = currentUser.Role,
        customRole = currentUser.CustomRole,
        permissions = currentUser.Permissions,
        staffId = currentUser.StaffId,
        classes = await classes.ClassesOfAsync(currentUser.StaffId, ct),
        mustChangePassword = currentUser.MustChangePassword,
    };

    /// <summary>Anyone signed in; the only thing a temporary password may do.</summary>
    public async Task ChangePasswordAsync(string? currentPassword, JsonNode? newPassword, CancellationToken ct = default)
    {
        var next = newPassword is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;
        if (next is null || next.Length < 8) throw HttpException.BadRequest("The new password must be at least 8 characters");
        if (next == currentPassword) throw HttpException.BadRequest("Choose a password different from the current one");
        var user = await users.FindExistingAsync(currentUser.Id, ct);
        if (!hasher.Verify(currentPassword ?? "", user.PasswordHash))
            throw HttpException.BadRequest("The current password is not correct");
        user.PasswordHash = hasher.Hash(next);
        user.MustChangePassword = false;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
