using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Identity;

/// <summary>The "JwtSettings" section of appsettings.</summary>
public sealed class JwtSettings
{
    public const string Section = "JwtSettings";

    /// <summary>HS256 signing key; at least 32 characters.</summary>
    public string SecretKey { get; set; } = "";

    public string Issuer { get; set; } = "SchoolERP.API";
    public string Audience { get; set; } = "SchoolERP.Client";
    public double ExpiryMinutes { get; set; } = 480;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(SecretKey));
}

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    // Same cost as the Node app (bcrypt.hash(password, 10)); $2a$/$2b$ hashes from bcryptjs verify unchanged.
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, 10);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}

public sealed class JwtTokenService(JwtSettings settings) : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public string CreateToken(int id, string name, string role)
    {
        var now = DateTime.UtcNow;
        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            // the Node app's payload { id, name, role, iat, exp }, plus iss and aud
            Claims = new Dictionary<string, object> { ["id"] = id, ["name"] = name, ["role"] = role },
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now,
            NotBefore = null,
            Expires = now.AddMinutes(settings.ExpiryMinutes),
            SigningCredentials = new SigningCredentials(settings.SigningKey(), SecurityAlgorithms.HmacSha256),
        });
    }
}

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
