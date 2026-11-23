using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Salary terms (EF Core).</summary>
public interface IStaffSalaryRepository : IRepository<StaffSalary>
{
    /// <summary>Tracked salary terms of one staff member, or null.</summary>
    Task<StaffSalary?> FindByStaffAsync(int staffId, CancellationToken ct = default);

    /// <summary>Salary terms of active staff (with a live staff record).</summary>
    Task<List<StaffSalary>> ForActiveStaffAsync(CancellationToken ct = default);
}
