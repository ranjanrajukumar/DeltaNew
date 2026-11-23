using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Role rights switched off per role and module (EF Core).</summary>
public interface IRoleRightRepository : IRepository<RoleRight>
{
    /// <summary>Every row, read-only.</summary>
    Task<List<RoleRight>> AllAsync(CancellationToken ct = default);
}
