using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Homework and submissions (EF Core).</summary>
public interface IHomeworkRepository : IRepository<Homework>
{
    /// <summary>One homework with its class and subject.</summary>
    Task<Homework?> GetWithNamesAsync(int id, CancellationToken ct = default);

    /// <summary>The highest mark given for a homework, or null.</summary>
    Task<decimal?> HighestMarkAsync(int homeworkId, CancellationToken ct = default);

    Task<List<HomeworkSubmission>> SubmissionsAsync(int homeworkId, CancellationToken ct = default);

    /// <summary>Tracked submission of one student, or null.</summary>
    Task<HomeworkSubmission?> FindSubmissionAsync(int homeworkId, int studentId, CancellationToken ct = default);

    void AddSubmission(HomeworkSubmission submission);
}
