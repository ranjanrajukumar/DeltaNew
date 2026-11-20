using SchoolERP.Application.Interfaces;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

/// <summary>The request's DbContext as the save and transaction boundary.</summary>
public sealed class UnitOfWork(SchoolErpDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct = default) => db.InTransactionAsync(work, ct);
}
