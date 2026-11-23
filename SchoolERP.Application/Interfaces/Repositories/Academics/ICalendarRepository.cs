using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Calendar events (EF Core).</summary>
public interface ICalendarRepository : IRepository<CalendarEvent>
{
    /// <summary>Events overlapping [from, to]; only these audiences when given.</summary>
    Task<List<CalendarEvent>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences, CancellationToken ct = default);

    /// <summary>Activities that are not cancelled overlapping [from, to]; only those open to every class when <paramref name="schoolWideOnly"/>.</summary>
    Task<List<Activity>> ActivitiesBetweenAsync(DateOnly from, DateOnly to, bool schoolWideOnly, CancellationToken ct = default);

    /// <summary>Holidays (for everyone or this audience) overlapping [from, to].</summary>
    Task<List<CalendarEvent>> HolidaysAsync(DateOnly from, DateOnly to, string audience, CancellationToken ct = default);
}
