using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Browser push subscriptions (EF Core; hard-deleted).</summary>
public interface IPushSubscriptionRepository : IRepository<PushSubscription>
{
    /// <summary>The subscription with this endpoint, tracked; null when none.</summary>
    Task<PushSubscription?> FindByEndpointAsync(string endpoint, CancellationToken ct = default);

    /// <summary>One user's subscriptions with this endpoint, tracked.</summary>
    Task<List<PushSubscription>> ListOfUserAsync(int userId, string endpoint, CancellationToken ct = default);

    Task<int> CountOfUserAsync(int userId, CancellationToken ct = default);

    /// <summary>The subscriptions of these users, tracked.</summary>
    Task<List<PushSubscription>> ListOfUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken ct = default);

    Task<int> CountOfUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken ct = default);
}
