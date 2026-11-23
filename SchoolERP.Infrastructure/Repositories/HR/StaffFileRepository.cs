using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StaffFileRepository(ApplicationDbContext db) : Repository<StaffFile>(db), IStaffFileRepository
{
    public Task<List<StaffFile>> ListWithoutDataAsync(int staffId, CancellationToken ct = default) =>
        Read.Where(f => f.StaffId == staffId).OrderByDescending(f => f.Id)
            .Select(f => new StaffFile
            {
                Id = f.Id, StaffId = f.StaffId, Title = f.Title, OriginalName = f.OriginalName, MimeType = f.MimeType, Size = f.Size,
                StoredName = f.StoredName, UploadedByName = f.UploadedByName, CreatedAt = f.CreatedAt,
            }).ToListAsync(ct);
}
