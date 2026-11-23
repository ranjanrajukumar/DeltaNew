using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Infrastructure.Identity;

namespace SchoolERP.API.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>JWT bearer authentication with the "JwtSettings" section (HS256, issuer, audience, lifetime, no clock skew).</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.Section).Get<JwtSettings>() ?? new JwtSettings();
        if (jwtSettings.SecretKey.Length < 32)
            throw new InvalidOperationException(
                "Set JwtSettings:SecretKey to a private random value of at least 32 characters (appsettings or environment variable JwtSettings__SecretKey).");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false; // keep the token's own claim names: id, name, role
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey = jwtSettings.SigningKey(),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

                ClockSkew = TimeSpan.Zero,
            };
        });
        services.AddAuthorization();

        return services;
    }

    /// <summary>The browser apps allowed to call the API: Cors:ClientOrigins in appsettings; when empty, any origin.</summary>
    public static IServiceCollection AddClientCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:ClientOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy("ClientApp", policy =>
        {
            if (origins.Length > 0) policy.WithOrigins(origins);
            else policy.AllowAnyOrigin();
            policy.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Retry-After", "Content-Disposition");
        }));
        return services;
    }
}
