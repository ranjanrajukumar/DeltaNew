using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Online classes (EF Core).</summary>
public interface IOnlineClassRepository : IRepository<OnlineClass>
{
    /// <summary>With class and subject; "upcoming" (from <paramref name="today"/>, soonest first), "past" (newest first) or all; at most 300.</summary>
    Task<List<OnlineClass>> ListAsync(int? classId, string? when, DateOnly today, CancellationToken ct = default);

    Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default);

    Task<bool> SubjectExistsAsync(int subjectId, CancellationToken ct = default);
}
