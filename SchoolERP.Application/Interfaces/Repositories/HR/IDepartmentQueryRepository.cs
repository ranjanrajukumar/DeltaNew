using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Department lists with names (Dapper).</summary>
public interface IDepartmentQueryRepository
{
    public sealed record DepartmentRow(int Id, string Name, int? HeadStaffId, string? HeadName);

    /// <summary>Departments by name, with the head's "First Last" name (null when the head is not a live staff record).</summary>
    Task<List<DepartmentRow>> ListWithHeadsAsync(CancellationToken ct = default);
}
