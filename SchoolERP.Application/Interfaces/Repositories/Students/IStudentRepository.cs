using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Students and the rows that hang off them (EF Core).</summary>
public interface IStudentRepository : IRepository<Student>
{
    /// <summary>
    /// The list filters. A null value does not filter. <paramref name="Like"/> (a %text% pattern) matches names, admission and roll
    /// numbers and phones; <paramref name="ById"/> also matches that student id when searching.
    /// </summary>
    public sealed record ListFilter(
        string? Grade, string? Section, string? RegistrationStatus, int? ClassId, string? Status, string? Like, int? ById);

    /// <summary>Every matching student with class and class teacher, newest first, read-only.</summary>
    Task<List<Student>> ListWithClassAsync(ListFilter filter, CancellationToken ct = default);

    /// <summary>How many students match.</summary>
    Task<int> CountAsync(ListFilter filter, CancellationToken ct = default);

    /// <summary>One page of matching students with class and class teacher, newest first, read-only.</summary>
    Task<List<Student>> PageWithClassAsync(ListFilter filter, int skip, int take, CancellationToken ct = default);

    /// <summary>The student with class and class teacher, read-only; null when none.</summary>
    Task<Student?> GetWithClassAsync(int id, CancellationToken ct = default);

    /// <summary>Which of these students have a photo.</summary>
    Task<HashSet<int>> WithPhotoAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    Task<bool> ClassExistsAsync(int classId, CancellationToken ct = default);

    Task<int?> MaxIdAsync(CancellationToken ct = default);

    Task<bool> AdmissionNoExistsAsync(string admissionNo, CancellationToken ct = default);

    /// <summary>A student with this name and date of birth, read-only; null when none.</summary>
    Task<Student?> FindByNameAndDobAsync(string firstName, string lastName, DateOnly dob, CancellationToken ct = default);

    Task<bool> HasDocumentRequestsAsync(int studentId, CancellationToken ct = default);

    /// <summary>The stored file names of the student's uploaded files.</summary>
    Task<List<string>> StoredFileNamesAsync(int studentId, CancellationToken ct = default);

    /// <summary>The student's login link, read-only; null when none.</summary>
    Task<StudentAccount?> LoginOfAsync(int studentId, CancellationToken ct = default);

    /// <summary>Admissions enrolled as this student, tracked.</summary>
    Task<List<Admission>> EnrolledAdmissionsAsync(int studentId, CancellationToken ct = default);

    /// <summary>The photos of these kinds for this student, tracked.</summary>
    Task<List<Photo>> PhotosAsync(int studentId, IReadOnlyCollection<string> entities, CancellationToken ct = default);

    void RemovePhotos(IEnumerable<Photo> photos);
}
