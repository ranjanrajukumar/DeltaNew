using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Class–subject links (EF Core).</summary>
public interface IClassSubjectRepository : IRepository<ClassSubject>
{
    Task<bool> ExistsAsync(int classId, int subjectId, CancellationToken ct = default);

    /// <summary>Tracked link of a subject in a class, or null.</summary>
    Task<ClassSubject?> FindAsync(int classId, int subjectId, CancellationToken ct = default);

    /// <summary>The links of these classes (read-only).</summary>
    Task<List<ClassSubject>> OfClassesAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default);

    /// <summary>Every link that has a teacher (read-only).</summary>
    Task<List<ClassSubject>> WithTeacherAsync(CancellationToken ct = default);

    /// <summary>Classes with these ids, or every class when <paramref name="ids"/> is empty (read-only, unordered).</summary>
    Task<List<SchoolClass>> ClassesAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
}
