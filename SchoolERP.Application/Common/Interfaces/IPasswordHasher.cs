namespace SchoolERP.Application.Common.Interfaces;

/// <summary>bcrypt, compatible with the hashes the Node app (bcryptjs) stored.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
