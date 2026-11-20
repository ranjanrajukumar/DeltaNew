using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using static SchoolERP.Application.Interfaces.Repositories.ISubjectMapRepository;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SubjectMapRepository(SchoolErpDbContext db) : Repository<ClassSubject>(db), ISubjectMapRepository
{
    /// <summary>Filters run on the entity first: EF cannot translate a Where on a record built by a constructor.</summary>
    private Task<List<StudentRef>> StudentsWhereAsync(System.Linq.Expressions.Expression<Func<Student, bool>> filter, CancellationToken ct) =>
        Db.Set<Student>().AsNoTracking().Where(filter)
            .Select(s => new StudentRef(s.Id, s.AdmissionNo, s.FirstName, s.LastName, s.ClassId)).ToListAsync(ct);

    public Task<List<ClassSubject>> WithSubjectsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Read.Include(cs => cs.Subject).Where(cs => ids.Contains(cs.Id)).ToListAsync(ct);

    public Task<int> ActiveStudentCountAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().CountAsync(s => s.ClassId == classId && s.Status == "active", ct);

    public Task<List<StudentRef>> ActiveStudentsAsync(int classId, CancellationToken ct = default) =>
        Db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.FirstName)
            .Select(s => new StudentRef(s.Id, s.AdmissionNo, s.FirstName, s.LastName, s.ClassId)).ToListAsync(ct);

    public async Task<StudentRef?> StudentAsync(int studentId, CancellationToken ct = default) =>
        (await StudentsWhereAsync(s => s.Id == studentId, ct)).FirstOrDefault();

    public Task<List<StudentRef>> StudentsAsync(IReadOnlyCollection<int> studentIds, CancellationToken ct = default) =>
        StudentsWhereAsync(s => studentIds.Contains(s.Id), ct);

    public Task<List<int>> ClassSubjectIdsAsync(int classId, bool electivesOnly, CancellationToken ct = default) =>
        Set.Where(cs => cs.ClassId == classId && (!electivesOnly || cs.IsElective)).Select(cs => cs.Id).ToListAsync(ct);

    public async Task<List<(int Id, int SubjectId)>> ElectivesAsync(int classId, CancellationToken ct = default) =>
        (await Set.Where(cs => cs.ClassId == classId && cs.IsElective).Select(cs => new { cs.Id, cs.SubjectId }).ToListAsync(ct))
        .Select(x => (x.Id, x.SubjectId)).ToList();

    public Task<List<Choice>> ChoicesAsync(IReadOnlyCollection<int> classSubjectIds, IReadOnlyCollection<int>? studentIds = null, CancellationToken ct = default) =>
        Db.Set<StudentSubject>().AsNoTracking()
            .Where(ss => classSubjectIds.Contains(ss.ClassSubjectId) && (studentIds == null || studentIds.Contains(ss.StudentId)))
            .Select(ss => new Choice(ss.StudentId, ss.ClassSubjectId)).ToListAsync(ct);

    public Task<List<int>> ChosenClassSubjectIdsAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<StudentSubject>().Where(ss => ss.StudentId == studentId).Select(ss => ss.ClassSubjectId).ToListAsync(ct);

    public Task<List<StudentSubject>> StudentChoicesAsync(int studentId, IReadOnlyCollection<int>? classSubjectIds, CancellationToken ct = default) =>
        Db.Set<StudentSubject>().Where(ss => ss.StudentId == studentId && (classSubjectIds == null || classSubjectIds.Contains(ss.ClassSubjectId))).ToListAsync(ct);

    public Task<List<StudentSubject>> StaleChoicesAsync(int studentId, IReadOnlyCollection<int> keep, CancellationToken ct = default) =>
        Db.Set<StudentSubject>().Where(ss => ss.StudentId == studentId && (keep.Count == 0 || !keep.Contains(ss.ClassSubjectId))).ToListAsync(ct);

    public Task<List<StudentSubject>> ChoicesOfAsync(int classSubjectId, CancellationToken ct = default) =>
        Db.Set<StudentSubject>().Where(ss => ss.ClassSubjectId == classSubjectId).ToListAsync(ct);

    public void AddChoices(IEnumerable<StudentSubject> choices) => Db.Set<StudentSubject>().AddRange(choices);

    public void RemoveChoices(IEnumerable<StudentSubject> choices) => Db.Set<StudentSubject>().RemoveRange(choices);
}
