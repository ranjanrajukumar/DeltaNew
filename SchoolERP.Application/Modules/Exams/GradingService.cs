using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Exams;

/// <summary>A board's grading policy, loaded once per request (the Node app's exams/grading.js).</summary>
public sealed record GradingPolicy(string Board, IReadOnlyList<GradingPolicy.Band> Bands, double PassPercent, bool IsDefault, int Version, string? SourceRef)
{
    public sealed record Band(string Grade, double Min);

    /// <summary>The first band whose lower limit the percentage reaches, else the lowest band.</summary>
    public string GradeFor(double percent) => (Bands.FirstOrDefault(b => percent >= b.Min) ?? Bands[^1]).Grade;

    /// <summary>The bands as the API returns them: [{ grade, min }], highest first.</summary>
    public JsonArray BandsJson()
    {
        var list = new JsonArray();
        foreach (var b in Bands) list.Add(new JsonObject { ["grade"] = b.Grade, ["min"] = JsonDefaults.Node(b.Min == Math.Floor(b.Min) ? (object)(long)b.Min : b.Min) });
        return list;
    }
}

public sealed class GradingService(IApplicationDbContext db, IBoardService boards)
{
    /// <summary>
    /// The grading policy of a board in an academic session (default: the current one), from the board's grading rule. A missing
    /// rule is a 409 telling the administrator to add one.
    /// </summary>
    public async Task<GradingPolicy> LoadAsync(Board board, int? yearId = null, CancellationToken ct = default)
    {
        var rule = await boards.RequireRuleAsync(board, "grading", "default", yearId, ct);
        var bands = (rule.Payload?["bands"] as JsonArray ?? [])
            .Select(b => new GradingPolicy.Band(Body.AsString(b?["grade"]) ?? "", Body.AsNumber(b?["min"]) ?? 0))
            .OrderByDescending(b => b.Min).ToList();
        return new GradingPolicy(board.Name, bands, Body.AsNumber(rule.Payload?["passPercent"]) ?? 0, !rule.Verified, rule.Version, rule.SourceRef);
    }

    /// <summary>A board by name or id ("CBSE" when not given). An unknown board is a 409 like a board without a rule.</summary>
    public async Task<GradingPolicy> LoadAsync(string? boardRef, CancellationToken ct = default)
    {
        var name = string.IsNullOrEmpty(boardRef) ? "CBSE" : boardRef;
        var board = await boards.FindAsync(name, ct)
            ?? throw Common.Exceptions.HttpException.Conflict($"No grading rule is set for {name}. Add one under Board rules");
        return await LoadAsync(board, ct: ct);
    }

    /// <summary>The grading policy of the board a class follows.</summary>
    public async Task<GradingPolicy> ForClassAsync(int? classId, CancellationToken ct = default)
    {
        var cls = classId is { } id ? await db.Set<SchoolClass>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct) : null;
        var board = await boards.OfClassAsync(cls, ct) ?? throw Common.Exceptions.HttpException.Conflict("Add a board first");
        return await LoadAsync(board, ct: ct);
    }
}
