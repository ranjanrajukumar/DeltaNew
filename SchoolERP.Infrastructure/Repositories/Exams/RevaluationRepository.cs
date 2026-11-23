using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class RevaluationRepository(ApplicationDbContext db) : Repository<RevaluationRequest>(db), IRevaluationRepository
{
    public Task<List<RevaluationRequest>> ListAsync(string? status, int? studentId, int? examId, CancellationToken ct = default) =>
        Read.Include(r => r.Exam).Include(r => r.Subject).Include(r => r.Student)
            .Where(r => r.Exam != null && r.Subject != null && r.Student != null
                && (string.IsNullOrEmpty(status) || r.Status == status) && (studentId == null || r.StudentId == studentId) && (examId == null || r.ExamId == examId))
            .OrderByDescending(r => r.Id).Take(300).ToListAsync(ct);

    public Task<bool> PendingExistsAsync(int? examId, int? studentId, int? subjectId, CancellationToken ct = default) =>
        Set.AnyAsync(r => r.ExamId == examId && r.StudentId == studentId && r.SubjectId == subjectId && r.Status == "pending", ct);

    public Task<RevaluationRequest?> FindWithExamAsync(int id, CancellationToken ct = default) =>
        Set.Include(r => r.Exam).FirstOrDefaultAsync(r => r.Id == id, ct);
}
