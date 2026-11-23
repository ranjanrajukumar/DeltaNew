namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// Signs the sign-in tokens (HS256 JWT with { id, name, role } plus issuer and audience, JwtSettings:ExpiryMinutes long).
/// Checking them is the JwtBearer authentication handler's job (see Program.cs).
/// </summary>
public interface ITokenService
{
    string CreateToken(int id, string name, string role);
}
