using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Revaluation requests (EF Core).</summary>
public interface IRevaluationRepository : IRepository<RevaluationRequest>
{
    /// <summary>Requests with exam, subject and student (all must exist), filtered, newest first, at most 300.</summary>
    Task<List<RevaluationRequest>> ListAsync(string? status, int? studentId, int? examId, CancellationToken ct = default);

    Task<bool> PendingExistsAsync(int? examId, int? studentId, int? subjectId, CancellationToken ct = default);

    /// <summary>A tracked request with its exam, or null.</summary>
    Task<RevaluationRequest?> FindWithExamAsync(int id, CancellationToken ct = default);
}
