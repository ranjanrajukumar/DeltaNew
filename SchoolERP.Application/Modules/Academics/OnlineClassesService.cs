using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Application.Interfaces.Services;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>
/// Live classes over a video link, scheduled by admins and teachers (the Node app's academics/onlineClasses.routes.js).
/// Students see theirs through the portal.
/// </summary>
public sealed partial class OnlineClassesService(IOnlineClassRepository onlineClasses, IUnitOfWork unitOfWork, ICurrentUser user) : IOnlineClassService
{
    private static readonly string[] Fields = ["title", "classId", "subjectId", "classDate", "startTime", "endTime", "link", "teacherName", "notes"];

    public static JsonObject Shape(OnlineClass c) => new()
    {
        ["id"] = c.Id,
        ["title"] = c.Title,
        ["classId"] = c.ClassId,
        ["className"] = c.SchoolClass is { } s ? $"{s.Name}-{s.Section}" : null,
        ["subjectId"] = c.SubjectId,
        ["subjectName"] = c.Subject?.Name,
        ["classDate"] = c.ClassDate.ToString("yyyy-MM-dd"),
        ["startTime"] = c.StartTime,
        ["endTime"] = c.EndTime,
        ["link"] = c.Link,
        ["teacherName"] = c.TeacherName,
        ["notes"] = c.Notes,
    };

    /// <summary>Upcoming first for "upcoming", newest first for "past"; at most 300.</summary>
    public async Task<List<JsonObject>> ListAsync(int? classId, string? when, CancellationToken ct = default)
    {
        return (await onlineClasses.ListAsync(classId, when, Clock.Today(), ct)).Select(Shape).ToList();
    }

    /// <summary>
    /// The Node app's clean(): text is trimmed and blank text is null; then title, class, date, start time and link are
    /// required and checked. Returns the clean values as a body to copy onto the row.
    /// </summary>
    private async Task<Body> CleanAsync(JsonObject input, CancellationToken ct)
    {
        var d = new JsonObject();
        foreach (var f in Fields)
        {
            if (!input.TryGetPropertyValue(f, out var v)) continue;
            var text = (Body.AsString(v) ?? "").Trim();
            d[f] = text == "" ? null : v is JsonValue jv && jv.TryGetValue<string>(out var s) ? s.Trim() : v?.DeepClone();
        }
        var b = new Body(d);
        var title = b.Str("title");
        if (string.IsNullOrEmpty(title)) throw HttpException.BadRequest("Title is required");
        if (title.Length > 160) throw HttpException.BadRequest("Title must be 160 characters or fewer");
        if (b.Id("classId") is not { } classId || !await onlineClasses.ClassExistsAsync(classId, ct))
            throw HttpException.BadRequest("Choose the class this is for");
        if (b.Truthy("subjectId") && (b.Int("subjectId") is not { } subjectId || !await onlineClasses.SubjectExistsAsync(subjectId, ct)))
            throw HttpException.BadRequest("That subject does not exist");
        var date = b.Str("classDate");
        if (date is null || !IsoDate().IsMatch(date) || Body.AsDate(d["classDate"]) is null) throw HttpException.BadRequest("Date is required");
        var start = b.Str("startTime");
        if (start is null || !Time().IsMatch(start)) throw HttpException.BadRequest("Start time must look like 14:30");
        var end = b.Str("endTime");
        if (end is not null && (!Time().IsMatch(end) || string.CompareOrdinal(end, start) <= 0))
            throw HttpException.BadRequest("End time must look like 15:15 and be after the start time");
        var link = b.Str("link");
        if (link is null) throw HttpException.BadRequest("The joining link is required");
        if (!HttpLink().IsMatch(link) || link.Length > 500)
            throw HttpException.BadRequest("The joining link must be a web address starting with http:// or https://");
        if (b.Str("notes") is { Length: > 500 }) throw HttpException.BadRequest("Notes must be 500 characters or fewer");
        return b;
    }

    public async Task<int> CreateAsync(JsonObject? body, CancellationToken ct = default)
    {
        var input = new JsonObject { ["teacherName"] = user.Name };
        foreach (var (k, v) in body ?? []) input[k] = v?.DeepClone();
        var clean = await CleanAsync(input, ct);
        var row = new OnlineClass();
        CrudService.Apply(clean, Fields, row, blankAsNull: false);
        onlineClasses.Add(row);
        await unitOfWork.SaveChangesAsync(ct);
        return row.Id;
    }

    public async Task UpdateAsync(int id, JsonObject? body, CancellationToken ct = default)
    {
        var row = await onlineClasses.FindAsync(id, ct) ?? throw HttpException.NotFound("Online class not found");
        var input = JsonDefaults.ToJsonObject(row); // { ...row.toJSON(), ...req.body }
        foreach (var (k, v) in body ?? []) input[k] = v?.DeepClone();
        var clean = await CleanAsync(input, ct);
        CrudService.Apply(clean, Fields, row, blankAsNull: false);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var row = await onlineClasses.FindAsync(id, ct) ?? throw HttpException.NotFound("Online class not found");
        onlineClasses.Remove(row);
        await unitOfWork.SaveChangesAsync(ct);
    }

    [GeneratedRegex("^([01][0-9]|2[0-3]):[0-5][0-9]$")]
    private static partial Regex Time();

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex IsoDate();

    [GeneratedRegex(@"^https?://\S+$", RegexOptions.IgnoreCase)]
    private static partial Regex HttpLink();
}
