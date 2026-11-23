using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Student photos and documents (EF Core).</summary>
public interface IStudentFileRepository : IRepository<StudentFile>
{
    /// <summary>A student's files without their bytes, newest first; only one kind when given.</summary>
    Task<List<StudentFile>> ListWithoutDataAsync(int studentId, string? kind, CancellationToken ct = default);

    /// <summary>Tracked photo rows of a student.</summary>
    Task<List<StudentFile>> PhotosAsync(int studentId, CancellationToken ct = default);

    Task<bool> StudentExistsAsync(int studentId, CancellationToken ct = default);
}
