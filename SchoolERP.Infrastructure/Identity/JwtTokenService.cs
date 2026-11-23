using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Identity;

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
