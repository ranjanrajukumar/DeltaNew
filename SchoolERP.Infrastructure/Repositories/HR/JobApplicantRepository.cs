using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class JobApplicantRepository(ApplicationDbContext db) : Repository<JobApplicant>(db), IJobApplicantRepository
{
    public Task<int> HiredCountAsync(int openingId, CancellationToken ct = default) =>
        Set.CountAsync(a => a.OpeningId == openingId && a.Status == "hired", ct);

    public Task<bool> AnyForOpeningAsync(int openingId, CancellationToken ct = default) => Set.AnyAsync(a => a.OpeningId == openingId, ct);

    public Task<List<JobApplicant>> ListWithOpeningAsync(int? openingId, CancellationToken ct = default) =>
        Read.Include(a => a.JobOpening).Where(a => openingId == null || a.OpeningId == openingId).OrderByDescending(a => a.Id).ToListAsync(ct);
}
