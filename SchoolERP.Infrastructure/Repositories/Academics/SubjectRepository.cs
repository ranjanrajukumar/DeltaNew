using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SubjectRepository(ApplicationDbContext db) : Repository<Subject>(db), ISubjectRepository
{
    public Task<List<Subject>> ListTrackedAsync(CancellationToken ct = default) => Set.ToListAsync(ct);

    public Task<Dictionary<int, string>> NamesAsync(CancellationToken ct = default) => Read.ToDictionaryAsync(s => s.Id, s => s.Name, ct);
}
