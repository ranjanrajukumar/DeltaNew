using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Year-end promotion (the Node app's academics/promotion.routes.js and promotionEligibility.js).</summary>
public sealed class PromotionService(IApplicationDbContext db, IBoardService boards, ExamExtrasService examExtras, ISubjectMapService subjectMap)
{
    public sealed record Eligibility(int StudentId, double? Percentage, string? Result, double? Attendance, string Status, List<string> Reasons);

    /// <summary>
    /// Who meets the board's promotion rule in a class: overall percentage of the class's annual result group, an overall pass,
    /// and attendance. A check that cannot be made (no result group yet, no attendance taken) is reported as unknown, never as a
    /// pass or a fail. The decision to promote stays with the administrator.
    /// </summary>
    public async Task<(string Board, JsonObject? Rule, List<Eligibility> Rows)> EligibilityAsync(int classId, CancellationToken ct = default)
    {
        var cls = await db.Set<SchoolClass>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId, ct) ?? throw HttpException.NotFound("Class not found");
        var board = await boards.OfClassAsync(cls, ct) ?? throw HttpException.Conflict("Add a board first");
        var rule = await boards.ResolveRuleAsync(board, "promotion", ct: ct);
        if (rule is null) return (board.Name, null, []);
        var p = rule.Payload as JsonObject ?? [];
        double? Limit(string key) => p[key] is null ? null : Body.AsNumber(p[key]);
        var minOverall = Limit("minOverallPercent");
        var minAttendance = Limit("minAttendancePercent");
        var requirePass = p["requirePass"] is JsonValue rp && rp.TryGetValue<bool>(out var req) && req;

        var examRule = await boards.ResolveRuleAsync(board, "exam_pattern", ct: ct);
        var groupName = Body.AsString(examRule?.Payload?["groupName"]);
        var group = examRule is null ? null : await db.Set<ExamGroup>().AsNoTracking().FirstOrDefaultAsync(g => g.ClassId == classId && g.Name == groupName, ct);
        var results = group is null ? [] : (await examExtras.GroupResultsAsync(group.Id, ct)).Rows.ToDictionary(r => r.StudentId);

        var students = await db.Set<Student>().AsNoTracking().Where(s => s.ClassId == classId && s.Status == "active").OrderBy(s => s.Id).Select(s => s.Id).ToListAsync(ct);
        var marks = await db.Set<AttendanceMark>().AsNoTracking().Where(a => students.Contains(a.StudentId)).Select(a => new { a.StudentId, a.Status }).ToListAsync(ct);
        var days = marks.GroupBy(m => m.StudentId).ToDictionary(g => g.Key, g => (Total: g.Count(), Present: g.Count(m => m.Status != "absent")));

        var rows = students.Select(id =>
        {
            var r = results.GetValueOrDefault(id);
            double? attendance = days.TryGetValue(id, out var a) && a.Total > 0 ? Js.Percent1((double)a.Present / a.Total) : null;
            var reasons = new List<string>();
            var unknown = new List<string>();
            if (minOverall is { } mo)
            {
                if (r is null) unknown.Add("no result yet");
                else if (r.Percentage < mo) reasons.Add($"overall {Num(r.Percentage)}% is below {Num(mo)}%");
            }
            if (requirePass)
            {
                if (r is null || r.Result == "incomplete") unknown.Add("result incomplete");
                else if (r.Result == "fail") reasons.Add("failed a subject");
            }
            if (minAttendance is { } ma)
            {
                if (attendance is null) unknown.Add("no attendance recorded");
                else if (attendance < ma) reasons.Add($"attendance {Num(attendance.Value)}% is below {Num(ma)}%");
            }
            return new Eligibility(id, r?.Percentage, r?.Result, attendance,
                reasons.Count > 0 ? "not-eligible" : unknown.Count > 0 ? "unknown" : "eligible", [.. reasons, .. unknown]);
        }).ToList();

        var ruleJson = new JsonObject { ["version"] = rule.Version, ["verified"] = rule.Verified, ["sourceRef"] = rule.SourceRef };
        foreach (var (k, v) in p) ruleJson[k] = v?.DeepClone();
        return (board.Name, ruleJson, rows);
    }

    private static string Num(double n) => n.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public async Task<object> EligibilityJsonAsync(int classId, CancellationToken ct = default)
    {
        var (board, rule, rows) = await EligibilityAsync(classId, ct);
        return new
        {
            board,
            rule,
            rows = rows.Select(r => new { studentId = r.StudentId, percentage = r.Percentage, result = r.Result, attendance = r.Attendance, status = r.Status, reasons = r.Reasons }),
        };
    }

    /// <summary>
    /// Moves active students of fromClassId (or only the listed studentIds; with onlyEligible, only those eligible) to toClassId.
    /// With no toClassId they are marked inactive (graduated / left).
    /// </summary>
    public async Task<object> PromoteAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        if (!b.Truthy("fromClassId")) throw HttpException.BadRequest("From class is required");
        var from = b.Int("fromClassId");
        int? to = b.Truthy("toClassId") ? b.Int("toClassId") : null;
        if (b.Truthy("toClassId") && b.Num("toClassId") == b.Num("fromClassId")) throw HttpException.BadRequest("Choose a different target class");
        if (from is null || !await db.Set<SchoolClass>().AnyAsync(c => c.Id == from, ct)) throw HttpException.NotFound("From class not found");
        if (b.Truthy("toClassId") && (to is null || !await db.Set<SchoolClass>().AnyAsync(c => c.Id == to, ct))) throw HttpException.NotFound("Target class not found");

        await boards.AssertSameBoardAsync(from, to, json?["allowCrossBoard"] is JsonValue ac && ac.TryGetValue<bool>(out var cross) && cross, ct);

        List<int>? only = json?["studentIds"] is JsonArray list ? list.Select(n => Body.AsNumber(n) is { } x ? (int)x : 0).ToList() : null;
        if (json?["onlyEligible"] is JsonValue oe && oe.TryGetValue<bool>(out var onlyEligible) && onlyEligible)
        {
            var ok = (await EligibilityAsync(from.Value, ct)).Rows.Where(r => r.Status == "eligible").Select(r => r.StudentId).ToList();
            only = only is null ? ok : only.Where(ok.Contains).ToList();
        }

        var count = await db.InTransactionAsync(async () =>
        {
            var students = await db.Set<Student>().Where(s => s.ClassId == from && s.Status == "active" && (only == null || only.Contains(s.Id))).ToListAsync(ct);
            foreach (var s in students)
            {
                if (to is { } target) s.ClassId = target;
                else s.Status = "inactive";
            }
            await db.SaveChangesAsync(ct);
            return students.Select(s => s.Id).ToList();
        }, ct);
        await subjectMap.PruneElectivesAsync(count, ct);
        return new { moved = count.Count, graduated = to is null };
    }
}
