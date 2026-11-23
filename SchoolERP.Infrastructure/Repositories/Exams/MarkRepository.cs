using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class MarkRepository(ApplicationDbContext db) : Repository<Mark>(db), IMarkRepository
{
    public Task<List<Mark>> OfExamAsync(int examId, CancellationToken ct = default) =>
        Read.Where(m => m.ExamId == examId).ToListAsync(ct);

    public Task<List<Mark>> OfExamAsync(int examId, IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        Read.Where(m => m.ExamId == examId && studentIds.Contains(m.StudentId)).ToListAsync(ct);

    public Task<List<Mark>> OfStudentAsync(int examId, int studentId, CancellationToken ct = default) =>
        Read.Where(m => m.ExamId == examId && m.StudentId == studentId).ToListAsync(ct);

    public Task<Mark?> FindAsync(int? examId, int? studentId, int? subjectId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(m => m.ExamId == examId && m.StudentId == studentId && m.SubjectId == subjectId, ct);

    public Task<Mark?> GetAsync(int? examId, int? studentId, int? subjectId, CancellationToken ct = default) =>
        Read.FirstOrDefaultAsync(m => m.ExamId == examId && m.StudentId == studentId && m.SubjectId == subjectId, ct);
}
