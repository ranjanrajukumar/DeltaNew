using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Staff records.</summary>
public interface IStaffRepository : IRepository<Staff>
{
    /// <summary>Every staff member's "First Last" name by id.</summary>
    Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default);

    /// <summary>"First Last" of one staff member, or null.</summary>
    Task<string?> NameAsync(int id, CancellationToken ct = default);

    /// <summary>The highest staff id ever used, counting deleted rows; null when the table is empty.</summary>
    Task<int?> MaxIdIncludingDeletedAsync(CancellationToken ct = default);

    /// <summary>Whether a staff number is already used, counting deleted rows.</summary>
    Task<bool> StaffNoTakenAsync(string staffNo, CancellationToken ct = default);

    /// <summary>Every staff member with their department, newest first.</summary>
    Task<List<Staff>> ListWithDepartmentAsync(CancellationToken ct = default);

    /// <summary>One staff member with their department, read only, or null.</summary>
    Task<Staff?> GetWithDepartmentAsync(int id, CancellationToken ct = default);

    /// <summary>Active staff by first name; only this staff category when given.</summary>
    Task<List<Staff>> ActiveAsync(string? category = null, CancellationToken ct = default);

    /// <summary>Active staff with their department, by first name.</summary>
    Task<List<Staff>> ActiveWithDepartmentAsync(CancellationToken ct = default);

    /// <summary>Active staff with their salary terms, by first name.</summary>
    Task<List<Staff>> ActiveWithSalaryAsync(CancellationToken ct = default);

    /// <summary>Marks the staff row (if any) and its photos for deletion; saved with the unit of work.</summary>
    Task RemoveWithPhotosAsync(int id, CancellationToken ct = default);

    /// <summary>The id of the login linked to this staff member, or null.</summary>
    Task<int?> LoginUserIdAsync(int staffId, CancellationToken ct = default);
}
