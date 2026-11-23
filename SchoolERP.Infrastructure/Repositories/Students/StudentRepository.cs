using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudentRepository(ApplicationDbContext db) : Repository<Student>(db), IStudentRepository
{
    private IQueryable<Student> WithClass() => Read.Include(s => s.SchoolClass).ThenInclude(c => c!.ClassTeacher);

    /// <summary>The filters on the student entity, before anything is loaded or ordered.</summary>
    private static IQueryable<Student> Filter(IQueryable<Student> students, IStudentRepository.ListFilter f)
    {
        if (f.Grade is { } grade) students = students.Where(s => s.SchoolClass != null && s.SchoolClass.Name == grade);
        if (f.Section is { } section) students = students.Where(s => s.SchoolClass != null && s.SchoolClass.Section == section);
        if (f.RegistrationStatus is { } registrationStatus) students = students.Where(s => s.RegistrationStatus == registrationStatus);
        if (f.ClassId is { } classId) students = students.Where(s => s.ClassId == classId);
        if (f.Status is { } status) students = students.Where(s => s.Status == status);
        if (f.Like is { } like)
        {
            var byId = f.ById;
            students = students.Where(s =>
                EF.Functions.Like(s.AdmissionNo, like) || EF.Functions.Like(s.FirstName, like) || EF.Functions.Like(s.LastName, like)
                || EF.Functions.Like(s.GuardianPhone!, like) || EF.Functions.Like(s.FatherPhone!, like) || EF.Functions.Like(s.MotherPhone!, like)
                || EF.Functions.Like(s.RollNo!, like) || EF.Functions.Like(s.GuardianName!, like) || EF.Functions.Like(s.FatherName!, like)
                || EF.Functions.Like(s.MotherName!, like) || (byId != null && s.Id == byId));
        }
        return students.OrderByDescending(s => s.Id);
    }

    public Task<List<Student>> ListWithClassAsync(IStudentRepository.ListFilter filter, CancellationToken ct = default) =>
        Filter(WithClass(), filter).ToListAsync(ct);

    public Task<int> CountAsync(IStudentRepository.ListFilter filter, CancellationToken ct = default) =>
        Filter(WithClass(), filter).CountAsync(ct);

    public Task<List<Student>> PageWithClassAsync(IStudentRepository.ListFilter filter, int skip, int take, CancellationToken ct = default) =>
        Filter(WithClass(), filter).Skip(skip).Take(take).ToListAsync(ct);

    public Task<Student?> GetWithClassAsync(int id, CancellationToken ct = default) => WithClass().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<HashSet<int>> WithPhotoAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        ids.Count == 0
            ? []
            : (await Db.Set<Photo>().Where(p => p.Entity == "student" && ids.Contains(p.EntityId)).Select(p => p.EntityId).ToListAsync(ct)).ToHashSet();

    public Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default) => Db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct);

    public async Task<int?> MaxIdAsync(CancellationToken ct = default) => await Set.MaxAsync(s => (int?)s.Id, ct);

    public Task<bool> AdmissionNoExistsAsync(string admissionNo, CancellationToken ct = default) => Set.AnyAsync(s => s.AdmissionNo == admissionNo, ct);

    public Task<Student?> FindByNameAndDobAsync(string firstName, string lastName, DateOnly dob, CancellationToken ct = default) =>
        Read.FirstOrDefaultAsync(s => s.FirstName == firstName && s.LastName == lastName && s.Dob == dob, ct);

    public Task<bool> HasDocumentRequestsAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<DocumentRequest>().AnyAsync(d => d.StudentId == studentId, ct);

    public Task<List<string>> StoredFileNamesAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<StudentFile>().Where(f => f.StudentId == studentId).Select(f => f.StoredName).ToListAsync(ct);

    public Task<StudentAccount?> LoginOfAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<StudentAccount>().AsNoTracking().FirstOrDefaultAsync(a => a.StudentId == studentId, ct);

    public Task<List<Admission>> EnrolledAdmissionsAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<Admission>().Where(a => a.StudentId == studentId && a.Status == "enrolled").ToListAsync(ct);

    public Task<List<Photo>> PhotosAsync(int studentId, IReadOnlyCollection<string> entities, CancellationToken ct = default) =>
        Db.Set<Photo>().Where(p => entities.Contains(p.Entity) && p.EntityId == studentId).ToListAsync(ct);

    public void RemovePhotos(IEnumerable<Photo> photos) => Db.Set<Photo>().RemoveRange(photos);
}
