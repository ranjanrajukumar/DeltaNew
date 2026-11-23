using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Year-end promotion: classes and the students who move (EF Core).</summary>
public interface IPromotionRepository
{
    /// <summary>A class (read-only), or null.</summary>
    Task<SchoolClass?> ClassAsync(int classId, CancellationToken ct = default);

    Task<bool> ClassExistsAsync(int? classId, CancellationToken ct = default);

    /// <summary>Ids of the active students of a class, by id.</summary>
    Task<List<int>> ActiveStudentIdsAsync(int classId, CancellationToken ct = default);

    /// <summary>Tracked active students of a class (only these ids when <paramref name="only"/> is given).</summary>
    Task<List<Student>> ActiveStudentsToMoveAsync(int? classId, IReadOnlyCollection<int>? only, CancellationToken ct = default);
}
