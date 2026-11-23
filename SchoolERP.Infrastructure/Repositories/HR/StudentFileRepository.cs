using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StudentFileRepository(ApplicationDbContext db) : Repository<StudentFile>(db), IStudentFileRepository
{
    public Task<List<StudentFile>> ListWithoutDataAsync(int studentId, string? kind, CancellationToken ct = default) =>
        Read.Where(f => f.StudentId == studentId && (string.IsNullOrEmpty(kind) || f.Kind == kind))
            .OrderByDescending(f => f.Id).Select(f => new StudentFile
            {
                Id = f.Id, StudentId = f.StudentId, Kind = f.Kind, Title = f.Title, OriginalName = f.OriginalName, MimeType = f.MimeType,
                Size = f.Size, StoredName = f.StoredName, UploadedByName = f.UploadedByName, CreatedAt = f.CreatedAt,
            }).ToListAsync(ct);

    public Task<List<StudentFile>> PhotosAsync(int studentId, CancellationToken ct = default) =>
        Set.Where(f => f.StudentId == studentId && f.Kind == "photo").ToListAsync(ct);

    public Task<bool> StudentExistsAsync(int studentId, CancellationToken ct = default) =>
        Db.Set<Student>().AnyAsync(s => s.Id == studentId, ct);
}
