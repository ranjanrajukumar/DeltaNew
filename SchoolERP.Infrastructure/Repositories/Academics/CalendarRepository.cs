using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class CalendarRepository(ApplicationDbContext db) : Repository<CalendarEvent>(db), ICalendarRepository
{
    public Task<List<CalendarEvent>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences, CancellationToken ct = default) =>
        Read.Where(e => e.StartDate <= to && e.EndDate >= from && (audiences == null || audiences.Contains(e.Audience))).ToListAsync(ct);

    public Task<List<Activity>> ActivitiesBetweenAsync(DateOnly from, DateOnly to, bool schoolWideOnly, CancellationToken ct = default) =>
        Db.Set<Activity>().AsNoTracking()
            .Where(a => a.Status != "cancelled" && a.StartDate <= to && a.EndDate >= from && (!schoolWideOnly || a.ClassId == null))
            .ToListAsync(ct);

    public Task<List<CalendarEvent>> HolidaysAsync(DateOnly from, DateOnly to, string audience, CancellationToken ct = default) =>
        Read.Where(e => e.Type == "holiday" && (e.Audience == "all" || e.Audience == audience) && e.StartDate <= to && e.EndDate >= from).ToListAsync(ct);
}
