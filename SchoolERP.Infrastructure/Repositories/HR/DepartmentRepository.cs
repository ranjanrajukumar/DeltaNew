using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class DepartmentRepository(ApplicationDbContext db) : Repository<Department>(db), IDepartmentRepository
{
    public Task<List<Department>> AllAsync(CancellationToken ct = default) => Read.ToListAsync(ct);
}
