using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class HomeworkRepository(ApplicationDbContext db) : Repository<Homework>(db), IHomeworkRepository
{
    public Task<Homework?> GetWithNamesAsync(int id, CancellationToken ct = default) =>
        Read.Include(h => h.SchoolClass).Include(h => h.Subject)
            .FirstOrDefaultAsync(h => h.Id == id && h.SchoolClass != null && h.Subject != null, ct);

    public Task<decimal?> HighestMarkAsync(int homeworkId, CancellationToken ct = default) =>
        Db.Set<HomeworkSubmission>().Where(s => s.HomeworkId == homeworkId).MaxAsync(s => s.Marks, ct);

    public Task<List<HomeworkSubmission>> SubmissionsAsync(int homeworkId, CancellationToken ct = default) =>
        Db.Set<HomeworkSubmission>().AsNoTracking().Where(s => s.HomeworkId == homeworkId).ToListAsync(ct);

    public Task<HomeworkSubmission?> FindSubmissionAsync(int homeworkId, int studentId, CancellationToken ct = default) =>
        Db.Set<HomeworkSubmission>().FirstOrDefaultAsync(s => s.HomeworkId == homeworkId && s.StudentId == studentId, ct);

    public void AddSubmission(HomeworkSubmission submission) => Db.Set<HomeworkSubmission>().Add(submission);
}
