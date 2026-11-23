using System.Text.Json.Nodes;

namespace SchoolERP.Application.Common.Interfaces;

/// <summary>Writable side of the current user, filled in once by the authentication middleware.</summary>
public interface ICurrentUserSetter
{
    void Set(int id, string name, string role, int? staffId, bool mustChangePassword, string? customRole, JsonObject? permissions);
}
