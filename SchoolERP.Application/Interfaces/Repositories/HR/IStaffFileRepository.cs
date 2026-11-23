using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Staff documents (EF Core).</summary>
public interface IStaffFileRepository : IRepository<StaffFile>
{
    /// <summary>A staff member's files without their bytes, newest first.</summary>
    Task<List<StaffFile>> ListWithoutDataAsync(int staffId, CancellationToken ct = default);
}
