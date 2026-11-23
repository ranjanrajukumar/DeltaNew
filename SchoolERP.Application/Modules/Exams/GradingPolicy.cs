using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces.Repositories;
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
