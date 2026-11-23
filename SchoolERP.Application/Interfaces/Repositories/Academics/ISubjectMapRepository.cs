using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Class subjects and the electives students chose.</summary>
public interface ISubjectMapRepository : IRepository<ClassSubject>
{
    public sealed record StudentRef(int Id, string AdmissionNo, string FirstName, string LastName, int? ClassId);
    public sealed record Choice(int StudentId, int ClassSubjectId);

    /// <summary>Class subjects by id, with the subject loaded.</summary>
    Task<List<ClassSubject>> WithSubjectsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    Task<int> ActiveStudentCountAsync(int classId, CancellationToken ct = default);

    /// <summary>Active students of a class by first name.</summary>
    Task<List<StudentRef>> ActiveStudentsAsync(int classId, CancellationToken ct = default);

    Task<StudentRef?> StudentAsync(int studentId, CancellationToken ct = default);

    Task<List<StudentRef>> StudentsAsync(IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>Ids of a class's subjects; only the electives when <paramref name="electivesOnly"/>.</summary>
    Task<List<int>> ClassSubjectIdsAsync(int classId, bool electivesOnly, CancellationToken ct = default);

    /// <summary>A class's electives as (class-subject id, subject id).</summary>
    Task<List<(int Id, int SubjectId)>> ElectivesAsync(int classId, CancellationToken ct = default);

    Task<List<Choice>> ChoicesAsync(IReadOnlyCollection<int> classSubjectIds, IReadOnlyCollection<int>? studentIds = null, CancellationToken ct = default);

    Task<List<int>> ChosenClassSubjectIdsAsync(int studentId, CancellationToken ct = default);

    /// <summary>Tracked choices of one student among these class subjects (null = any).</summary>
    Task<List<StudentSubject>> StudentChoicesAsync(int studentId, IReadOnlyCollection<int>? classSubjectIds, CancellationToken ct = default);

    /// <summary>Tracked choices of a student outside these class subjects (all when <paramref name="keep"/> is empty).</summary>
    Task<List<StudentSubject>> StaleChoicesAsync(int studentId, IReadOnlyCollection<int> keep, CancellationToken ct = default);

    /// <summary>Tracked choices of one class subject.</summary>
    Task<List<StudentSubject>> ChoicesOfAsync(int classSubjectId, CancellationToken ct = default);

    void AddChoices(IEnumerable<StudentSubject> choices);

    void RemoveChoices(IEnumerable<StudentSubject> choices);
}
