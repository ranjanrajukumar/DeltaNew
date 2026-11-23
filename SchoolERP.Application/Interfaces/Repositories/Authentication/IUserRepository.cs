using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Login users (EF Core).</summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>The user with this email, read-only; null when none.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>The login user of the student with this admission number, read-only; null when none.</summary>
    Task<User?> GetByAdmissionNoAsync(string admissionNo, CancellationToken ct = default);

    /// <summary>The user with their custom role loaded, read-only; null when none.</summary>
    Task<User?> GetWithRoleAsync(int id, CancellationToken ct = default);

    /// <summary>The user, tracked; throws when the user does not exist.</summary>
    Task<User> FindExistingAsync(int id, CancellationToken ct = default);

    /// <summary>Ids of the users with one of these roles (every user when <paramref name="roles"/> is null).</summary>
    Task<List<int>> IdsWithRolesAsync(IReadOnlyCollection<string>? roles, CancellationToken ct = default);
}
