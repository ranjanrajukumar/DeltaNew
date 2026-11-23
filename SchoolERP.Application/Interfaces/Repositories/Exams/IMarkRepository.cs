using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Marks (EF Core).</summary>
public interface IMarkRepository : IRepository<Mark>
{
    /// <summary>Every mark of an exam (read-only).</summary>
    Task<List<Mark>> OfExamAsync(int examId, CancellationToken ct = default);

    /// <summary>The marks of an exam for these students (read-only).</summary>
    Task<List<Mark>> OfExamAsync(int examId, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>One student's marks in an exam (read-only).</summary>
    Task<List<Mark>> OfStudentAsync(int examId, int studentId, CancellationToken ct = default);

    /// <summary>The tracked mark of a student in an exam and subject, or null.</summary>
    Task<Mark?> FindAsync(int? examId, int? studentId, int? subjectId, CancellationToken ct = default);

    /// <summary>The mark of a student in an exam and subject (read-only), or null.</summary>
    Task<Mark?> GetAsync(int? examId, int? studentId, int? subjectId, CancellationToken ct = default);
}
