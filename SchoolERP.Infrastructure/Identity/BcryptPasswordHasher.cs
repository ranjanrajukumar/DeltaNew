using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Identity;

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
