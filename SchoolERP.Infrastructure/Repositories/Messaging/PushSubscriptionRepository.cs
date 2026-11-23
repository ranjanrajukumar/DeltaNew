using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class PushSubscriptionRepository(ApplicationDbContext db) : Repository<PushSubscription>(db), IPushSubscriptionRepository
{
    public Task<PushSubscription?> FindByEndpointAsync(string endpoint, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(s => s.Endpoint == endpoint, ct);

    public Task<List<PushSubscription>> ListOfUserAsync(int userId, string endpoint, CancellationToken ct = default) =>
        Set.Where(s => s.UserId == userId && s.Endpoint == endpoint).ToListAsync(ct);

    public Task<int> CountOfUserAsync(int userId, CancellationToken ct = default) => Set.CountAsync(s => s.UserId == userId, ct);

    public Task<List<PushSubscription>> ListOfUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken ct = default) =>
        Set.Where(s => userIds.Contains(s.UserId)).ToListAsync(ct);

    public Task<int> CountOfUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken ct = default) =>
        Set.CountAsync(s => userIds.Contains(s.UserId), ct);
}
