using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class PerformanceReviewRepository(ApplicationDbContext db) : Repository<PerformanceReview>(db), IPerformanceReviewRepository
{
    public Task<List<PerformanceReview>> ListWithStaffAsync(int? staffId, CancellationToken ct = default) =>
        Read.Include(r => r.Staff).Where(r => r.Staff != null && (staffId == null || r.StaffId == staffId))
            .OrderByDescending(r => r.ReviewDate).ThenByDescending(r => r.Id).ToListAsync(ct);
}
