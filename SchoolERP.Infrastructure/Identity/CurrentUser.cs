using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Identity;

/// <summary>One per request; filled in by UserContextMiddleware once the token and the user row check out.</summary>
public sealed class CurrentUser : ICurrentUser, ICurrentUserSetter
{
    public bool IsAuthenticated { get; private set; }
    public int Id { get; private set; }
    public string? Name { get; private set; }
    public string? Role { get; private set; }
    public int? StaffId { get; private set; }
    public bool MustChangePassword { get; private set; }
    public string? CustomRole { get; private set; }
    public JsonObject? Permissions { get; private set; }

    public bool IsInRole(params string[] roles) => IsAuthenticated && Role is not null && roles.Contains(Role);

    public void Set(int id, string name, string role, int? staffId, bool mustChangePassword, string? customRole, JsonObject? permissions)
    {
        (IsAuthenticated, Id, Name, Role, StaffId, MustChangePassword, CustomRole, Permissions) =
            (true, id, name, role, staffId, mustChangePassword, customRole, permissions);
    }
}
