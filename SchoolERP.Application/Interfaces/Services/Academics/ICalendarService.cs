using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>The school calendar (the Node app's academics/calendar.routes.js).</summary>
public interface ICalendarService
{
    /// <summary>Events and (read-only) activities overlapping [from, to]; <paramref name="audiences"/> narrows it for families.</summary>
    Task<List<JsonObject>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences = null, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
