using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class CalendarRepository(SchoolErpDbContext db) : Repository<CalendarEvent>(db), ICalendarRepository
{
    public Task<List<CalendarEvent>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences, CancellationToken ct = default) =>
        Read.Where(e => e.StartDate <= to && e.EndDate >= from && (audiences == null || audiences.Contains(e.Audience))).ToListAsync(ct);

    public Task<List<Activity>> ActivitiesBetweenAsync(DateOnly from, DateOnly to, bool schoolWideOnly, CancellationToken ct = default) =>
        Db.Set<Activity>().AsNoTracking()
            .Where(a => a.Status != "cancelled" && a.StartDate <= to && a.EndDate >= from && (!schoolWideOnly || a.ClassId == null))
            .ToListAsync(ct);

    public Task<List<CalendarEvent>> HolidaysAsync(DateOnly from, DateOnly to, string audience, CancellationToken ct = default) =>
        Read.Where(e => e.Type == "holiday" && (e.Audience == "all" || e.Audience == audience) && e.StartDate <= to && e.EndDate >= from).ToListAsync(ct);
}

public sealed class HomeworkRepository(SchoolErpDbContext db) : Repository<Homework>(db), IHomeworkRepository
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

public sealed class StudyMaterialRepository(SchoolErpDbContext db) : Repository<StudyMaterial>(db), IStudyMaterialRepository
{
    public Task<List<StudyMaterial>> ListWithNamesAsync(int? classId, int? subjectId, CancellationToken ct = default) =>
        Read.Include(m => m.SchoolClass).Include(m => m.Subject)
            .Where(m => m.SchoolClass != null && m.Subject != null
                && (classId == null || m.ClassId == classId) && (subjectId == null || m.SubjectId == subjectId))
            .OrderByDescending(m => m.Id).ToListAsync(ct);
}

public sealed class SyllabusRepository(SchoolErpDbContext db) : Repository<SyllabusTopic>(db), ISyllabusRepository
{
    public Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default) =>
        Read.Where(t => (classId == null || t.ClassId == classId) && (subjectId == null || t.SubjectId == subjectId))
            .OrderBy(t => t.ClassId).ThenBy(t => t.SubjectId).ThenBy(t => t.Sequence).ThenBy(t => t.Id)
            .ToListAsync(ct);
}

public sealed class OnlineClassRepository(SchoolErpDbContext db) : Repository<OnlineClass>(db), IOnlineClassRepository
{
    public Task<List<OnlineClass>> ListAsync(int? classId, string? when, DateOnly today, CancellationToken ct = default)
    {
        var q = Read.Include(c => c.SchoolClass).Include(c => c.Subject).Where(c => c.SchoolClass != null && (classId == null || c.ClassId == classId));
        if (when == "upcoming") q = q.Where(c => c.ClassDate >= today);
        if (when == "past") q = q.Where(c => c.ClassDate < today);
        q = when == "past"
            ? q.OrderByDescending(c => c.ClassDate).ThenByDescending(c => c.StartTime)
            : q.OrderBy(c => c.ClassDate).ThenBy(c => c.StartTime);
        return q.Take(300).ToListAsync(ct);
    }

    public Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default) => Db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct);

    public Task<bool> SubjectExistsAsync(int subjectId, CancellationToken ct = default) => Db.Set<Subject>().AnyAsync(s => s.Id == subjectId, ct);
}

public sealed class SubjectRepository(SchoolErpDbContext db) : Repository<Subject>(db), ISubjectRepository
{
    public Task<List<Subject>> ListTrackedAsync(CancellationToken ct = default) => Set.ToListAsync(ct);

    public Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default) => Read.ToDictionaryAsync(s => s.Id, s => s.Name, ct);
}

public sealed class ClassSubjectRepository(SchoolErpDbContext db) : Repository<ClassSubject>(db), IClassSubjectRepository
{
    public Task<bool> ExistsAsync(int classId, int subjectId, CancellationToken ct = default) =>
        Set.AnyAsync(l => l.ClassId == classId && l.SubjectId == subjectId, ct);

    public Task<ClassSubject?> FindAsync(int classId, int subjectId, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(l => l.ClassId == classId && l.SubjectId == subjectId, ct);

    public Task<List<ClassSubject>> OfClassesAsync(IReadOnlyCollection<int> classIds, CancellationToken ct = default) =>
        Read.Where(l => classIds.Contains(l.ClassId)).ToListAsync(ct);

    public Task<List<ClassSubject>> WithTeacherAsync(CancellationToken ct = default) => Read.Where(l => l.StaffId != null).ToListAsync(ct);
}

public sealed class StudentLookupRepository(SchoolErpDbContext db) : IStudentLookupRepository
{
    public Task<List<Student>> ActiveInClassAsync(int classId, CancellationToken ct = default) =>
        db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);
}
