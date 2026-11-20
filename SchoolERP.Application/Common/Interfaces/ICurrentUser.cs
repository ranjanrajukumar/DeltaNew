using System.Text.Json.Nodes;

namespace SchoolERP.Application.Common.Interfaces;

/// <summary>The signed-in user of the current request (the Node app's req.user). Empty outside a signed-in request.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int Id { get; }
    string? Name { get; }

    /// <summary>The base role: admin, teacher, accountant, parent or student (a custom role reports its base role here).</summary>
    string? Role { get; }

    int? StaffId { get; }
    bool MustChangePassword { get; }

    /// <summary>The custom role's name, or null for a built-in role.</summary>
    string? CustomRole { get; }

    /// <summary>The custom role's { moduleKey: "read" | "write" } map, or null for a built-in role.</summary>
    JsonObject? Permissions { get; }

    bool IsInRole(params string[] roles);
}

/// <summary>Writable side of the current user, filled in once by the authentication middleware.</summary>
public interface ICurrentUserSetter
{
    void Set(int id, string name, string role, int? staffId, bool mustChangePassword, string? customRole, JsonObject? permissions);
}
