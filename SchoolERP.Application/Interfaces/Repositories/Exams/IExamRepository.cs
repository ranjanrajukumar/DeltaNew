using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Exams, plus the class, student, staff and subject lookups the exam screens need (EF Core).</summary>
public interface IExamRepository : IRepository<Exam>
{
    /// <summary>Exams whose class exists, with the class loaded, newest first.</summary>
    Task<List<Exam>> ListWithClassAsync(CancellationToken ct = default);

    /// <summary>The exams of a group, by id.</summary>
    Task<List<Exam>> InGroupAsync(int groupId, CancellationToken ct = default);

    /// <summary>The names of a class's exams.</summary>
    Task<List<string>> NamesInClassAsync(int classId, CancellationToken ct = default);

    /// <summary>The subjects taught in a class (subject loaded; links to a missing subject skipped), by subject id.</summary>
    Task<List<ClassSubject>> ClassSubjectsAsync(int classId, CancellationToken ct = default);

    /// <summary>The ids of the subjects these staff members teach in a class.</summary>
    Task<List<int>> SubjectIdsTaughtAsync(int classId, IReadOnlyCollection<int> staffIds, CancellationToken ct = default);

    /// <summary>A login's email, or null.</summary>
    Task<string?> UserEmailAsync(int userId, CancellationToken ct = default);

    /// <summary>Ids of active staff with this email.</summary>
    Task<List<int>> ActiveStaffIdsByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>"First Last" of a staff member, or null.</summary>
    Task<string?> StaffNameAsync(int staffId, CancellationToken ct = default);

    /// <summary>A class (read-only), or null.</summary>
    Task<SchoolClass?> ClassAsync(int classId, CancellationToken ct = default);

    /// <summary>These classes (every class when <paramref name="ids"/> is empty), read-only.</summary>
    Task<List<SchoolClass>> ClassesAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    /// <summary>Active students of a class, by first name.</summary>
    Task<List<Student>> ActiveStudentsAsync(int classId, CancellationToken ct = default);

    /// <summary>Every student of a class (any status), by id.</summary>
    Task<List<Student>> StudentsInClassAsync(int classId, CancellationToken ct = default);

    /// <summary>Ids of every student of a class (any status).</summary>
    Task<List<int>> StudentIdsInClassAsync(int classId, CancellationToken ct = default);

    /// <summary>A student with their class, or null.</summary>
    Task<Student?> StudentWithClassAsync(int studentId, CancellationToken ct = default);
}
