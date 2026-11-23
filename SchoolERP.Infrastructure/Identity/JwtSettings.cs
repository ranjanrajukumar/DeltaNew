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
