using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Staff records.</summary>
public interface IStaffRepository : IRepository<Staff>
{
    /// <summary>Every staff member's "First Last" name by id.</summary>
    Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default);

    /// <summary>"First Last" of one staff member, or null.</summary>
    Task<string?> NameAsync(int id, CancellationToken ct = default);
}
