using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>The school calendar (the Node app's academics/calendar.routes.js).</summary>
public sealed class CalendarService(ICalendarRepository calendar, IUnitOfWork unitOfWork) : ICalendarService
{
    public static readonly string[] EventTypes = ["holiday", "exam", "event", "meeting", "other"];
    private static readonly string[] Fields = ["title", "type", "startDate", "endDate", "audience", "description"];

    private static void Check(CalendarEvent e)
    {
        if (!EventTypes.Contains(e.Type)) throw HttpException.BadRequest($"Type must be one of {string.Join(", ", EventTypes)}");
        if (e.StartDate != default && e.EndDate != default && e.EndDate < e.StartDate)
            throw HttpException.BadRequest("The end date cannot be before the start date");
    }

    /// <summary>
    /// Events that overlap [from, to]. Staff see everything; <paramref name="audiences"/> narrows it for families. School
    /// activities appear too, read-only; families only see those open to every class. Sorted by start date, then id.
    /// </summary>
    public async Task<List<JsonObject>> EventsBetweenAsync(DateOnly from, DateOnly to, IReadOnlyCollection<string>? audiences = null, CancellationToken ct = default)
    {
        var events = await calendar.EventsBetweenAsync(from, to, audiences, ct);
        var activities = await calendar.ActivitiesBetweenAsync(from, to, schoolWideOnly: audiences is not null, ct);
        var fromActivities = activities.Select(a => new JsonObject
        {
            ["id"] = $"activity-{a.Id}",
            ["title"] = a.Title,
            ["type"] = "event",
            ["startDate"] = a.StartDate.ToString("yyyy-MM-dd"),
            ["endDate"] = a.EndDate.ToString("yyyy-MM-dd"),
            ["audience"] = "all",
            ["description"] = a.Venue is { Length: > 0 } venue ? $"At {venue}" : null,
            ["readOnly"] = true,
            ["source"] = "activity",
            ["activityId"] = a.Id,
        });
        return events.Select(JsonDefaults.ToJsonObject).Concat(fromActivities)
            .OrderBy(x => x["startDate"]?.GetValue<string>() ?? "", StringComparer.Ordinal)
            .ThenBy(x => Body.AsString(x["id"]) ?? "", StringComparer.InvariantCulture)
            .ToList();
    }

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        // endDate defaults to startDate; title and start date are required
        if (body.IsBlankTrimmed("title") || !body.Truthy("startDate")) throw HttpException.BadRequest("Title and start date are required");
        var e = new CalendarEvent();
        CrudService.Apply(body, Fields, e);
        if (!body.Truthy("endDate")) e.EndDate = e.StartDate;
        Check(e);
        calendar.Add(e);
        await unitOfWork.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var e = await calendar.FindAsync(id, ct) ?? throw HttpException.NotFound("Event not found");
        if (body.Has("type") && !EventTypes.Contains(body.Str("type") ?? ""))
            throw HttpException.BadRequest($"Type must be one of {string.Join(", ", EventTypes)}");
        CrudService.Apply(body, Fields, e);
        Check(e);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await calendar.FindAsync(id, ct) ?? throw HttpException.NotFound("Event not found");
        calendar.Remove(e);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
