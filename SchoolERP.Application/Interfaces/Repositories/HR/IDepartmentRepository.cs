using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Departments (EF Core).</summary>
public interface IDepartmentRepository : IRepository<Department>
{
    /// <summary>Every department, in table order (no sorting).</summary>
    Task<List<Department>> AllAsync(CancellationToken ct = default);
}
