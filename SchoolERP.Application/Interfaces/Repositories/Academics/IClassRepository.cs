using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Classes: writes and simple reads (EF Core). Lists with joins and counts are in <see cref="IClassQueryRepository"/>.</summary>
public interface IClassRepository : IRepository<SchoolClass>
{
    /// <summary>Classes ordered by name then section.</summary>
    Task<List<SchoolClass>> ListByNameAsync(CancellationToken ct = default);

    /// <summary>Classes with these ids, by id (all classes when <paramref name="ids"/> is empty).</summary>
    Task<List<SchoolClass>> ListByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    Task<bool> HasStudentsAsync(int classId, CancellationToken ct = default);

    Task<bool> HasExamsAsync(int classId, CancellationToken ct = default);

    /// <summary>The classes a staff member is class teacher of, by name then section.</summary>
    Task<List<SchoolClass>> ClassesOfTeacherAsync(int staffId, CancellationToken ct = default);
}
