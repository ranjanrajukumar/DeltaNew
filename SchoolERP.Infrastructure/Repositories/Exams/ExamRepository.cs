using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ExamRepository(ApplicationDbContext db) : Repository<Exam>(db), IExamRepository
{
    public Task<List<Exam>> ListWithClassAsync(CancellationToken ct = default) =>
        Read.Include(e => e.SchoolClass).Where(e => e.SchoolClass != null).OrderByDescending(e => e.Id).ToListAsync(ct);

    public Task<List<Exam>> InGroupAsync(int groupId, CancellationToken ct = default) =>
        Read.Where(e => e.ExamGroupId == groupId).OrderBy(e => e.Id).ToListAsync(ct);

    public Task<List<string>> NamesInClassAsync(int classId, CancellationToken ct = default) =>
        Set.Where(e => e.ClassId == classId).Select(e => e.Name).ToListAsync(ct);

    public Task<List<ClassSubject>> ClassSubjectsAsync(int classId, CancellationToken ct = default) =>
        Db.Set<ClassSubject>().AsNoTracking().Include(c => c.Subject)
            .Where(c => c.ClassId == classId && c.Subject != null).OrderBy(c => c.SubjectId).ToListAsync(ct);

    public Task<List<int>> SubjectIdsTaughtAsync(int classId, IReadOnlyCollection<int> staffIds, CancellationToken ct = default) =>
        Db.Set<ClassSubject>().Where(c => c.ClassId == classId && c.StaffId != null && staffIds.Contains(c.StaffId.Value))
            .Select(c => c.SubjectId).ToListAsync(ct);

    public Task<string?> UserEmailAsync(int userId, CancellationToken ct = default) =>
        Db.Set<User>().Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync(ct);

    public Task<List<int>> ActiveStaffIdsByEmailAsync(string email, CancellationToken ct = default) =>
        Db.Set<Staff>().Where(s => s.Email == email && s.Status == "active").Select(s => s.Id).ToListAsync(ct);

    public Task<string?> StaffNameAsync(int staffId, CancellationToken ct = default) =>
        Db.Set<Staff>().AsNoTracking().Where(s => s.Id == staffId).Select(s => s.FirstName + " " + s.LastName).FirstOrDefaultAsync(ct);

    public Task<SchoolClass?> ClassAsync(int classId, CancellationToken ct = default) =>
        Db.Set<SchoolClass>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId, ct);

    public Task<List<SchoolClass>> ClassesAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Db.Set<SchoolClass>().AsNoTracking().Where(c => ids.Count == 0 || ids.Contains(c.Id)).ToListAsync(ct);

    public Task<List<Student>> ActiveStudentsAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);

    public Task<List<Student>> StudentsInClassAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId).OrderBy(s => s.Id).ToListAsync(ct);

    public Task<List<int>> StudentIdsInClassAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().Where(s => s.ClassId == classId).Select(s => s.Id).ToListAsync(ct);

    public Task<Student?> StudentWithClassAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).FirstOrDefaultAsync(s => s.Id == studentId, ct);
}
