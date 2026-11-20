using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;

namespace SchoolERP.Application.Modules.Boards;

/// <summary>Checks and cleans a board rule's payload for each rule type (the Node app's VALIDATORS). No database work.</summary>
public static class BoardRuleValidator
{
    public static readonly string[] RuleTypes = ["subject_template", "exam_pattern", "grading", "bell_schedule", "promotion", "report_card"];

    private static double Num(JsonNode? v) =>
        v is null || (v is JsonValue s && s.TryGetValue<string>(out var t) && t == "") ? double.NaN : Body.AsNumber(v) ?? double.NaN;

    private static bool IsWhole(double n) => !double.IsNaN(n) && n == Math.Floor(n);

    /// <summary>Checks and cleans a rule payload of each type (the Node app's VALIDATORS).</summary>
    public static JsonNode ValidatePayload(string ruleType, string scopeKey, JsonNode? payload)
    {
        var p = payload as JsonObject ?? [];
        switch (ruleType)
        {
            case "subject_template":
            {
                if (!Academics.SubjectTemplates.Stages.Contains(scopeKey))
                    throw HttpException.BadRequest($"Scope must be one of: {string.Join(", ", Academics.SubjectTemplates.Stages)}");
                if (p["subjects"] is not JsonArray { Count: > 0 } subjects) throw HttpException.BadRequest("Add at least one subject");
                var seen = new HashSet<string>();
                var clean = new JsonArray();
                foreach (var s in subjects)
                {
                    var name = (Body.AsString(s?["name"]) ?? "").Trim();
                    var ppw = Num(s?["periodsPerWeek"]);
                    if (name == "" || name.Length > 80) throw HttpException.BadRequest("Every subject needs a name of up to 80 characters");
                    if (!seen.Add(name.ToLowerInvariant())) throw HttpException.BadRequest($"{name} is listed twice");
                    if (!IsWhole(ppw) || ppw < 1 || ppw > 12) throw HttpException.BadRequest($"{name}: periods per week must be a whole number from 1 to 12");
                    clean.Add(new JsonObject
                    {
                        ["name"] = name, ["periodsPerWeek"] = (int)ppw,
                        ["component"] = Body.AsString(s?["component"]) == "practical" ? "practical" : "theory",
                    });
                }
                return new JsonObject { ["subjects"] = clean };
            }
            case "exam_pattern":
            {
                if (p["exams"] is not JsonArray { Count: > 0 } exams) throw HttpException.BadRequest("Add at least one exam");
                var groupName = (Body.AsString(p["groupName"]) ?? "").Trim();
                if (groupName == "") throw HttpException.BadRequest("Give the result group a name");
                var skip = new JsonArray();
                foreach (var s in p["skipStages"] as JsonArray ?? [])
                    if (Body.AsString(s) is { } st && Academics.SubjectTemplates.Stages.Contains(st)) skip.Add(st);
                var clean = new JsonArray();
                foreach (var e in exams)
                {
                    var name = (Body.AsString(e?["name"]) ?? "").Trim();
                    var maxMarks = Num(e?["maxMarks"]);
                    var weight = Num(e?["weight"]);
                    if (name == "") throw HttpException.BadRequest("Every exam needs a name");
                    if (!IsWhole(maxMarks) || maxMarks < 1) throw HttpException.BadRequest($"{name}: max marks must be a whole number");
                    if (!IsWhole(weight) || weight < 1 || weight > 1000) throw HttpException.BadRequest($"{name}: weight must be a whole number from 1 to 1000");
                    clean.Add(new JsonObject { ["name"] = name, ["maxMarks"] = (int)maxMarks, ["weight"] = (int)weight });
                }
                return new JsonObject { ["groupName"] = groupName, ["skipStages"] = skip, ["exams"] = clean };
            }
            case "grading":
            {
                var pass = Num(p["passPercent"]);
                if (double.IsNaN(pass) || pass < 0 || pass > 100) throw HttpException.BadRequest("Pass mark must be between 0 and 100");
                if (p["bands"] is not JsonArray { Count: > 0 } bandsNode) throw HttpException.BadRequest("Add at least one grade");
                var bands = bandsNode.Select(b => (Grade: (Body.AsString(b?["grade"]) ?? "").Trim(), Min: Num(b?["min"]))).ToList();
                if (bands.Any(b => b.Grade == "" || b.Grade.Length > 10)) throw HttpException.BadRequest("Every grade needs a name of up to 10 characters");
                if (bands.Any(b => double.IsNaN(b.Min) || b.Min < 0 || b.Min > 100)) throw HttpException.BadRequest("Each lower limit must be between 0 and 100");
                if (bands.Select(b => b.Min).Distinct().Count() != bands.Count) throw HttpException.BadRequest("Two grades cannot start at the same percentage");
                if (bands.Select(b => b.Grade.ToLowerInvariant()).Distinct().Count() != bands.Count) throw HttpException.BadRequest("Grade names must be different");
                if (!bands.Any(b => b.Min == 0)) throw HttpException.BadRequest("The lowest grade must start at 0%");
                var sorted = new JsonArray();
                foreach (var b in bands.OrderByDescending(b => b.Min)) sorted.Add(new JsonObject { ["grade"] = b.Grade, ["min"] = JsNumber(b.Min) });
                return new JsonObject { ["passPercent"] = JsNumber(pass), ["bands"] = sorted };
            }
            case "promotion":
            {
                JsonNode? Limit(string key, string label)
                {
                    var v = p[key];
                    if (v is null || (v is JsonValue s && s.TryGetValue<string>(out var t) && t == "")) return null;
                    var n = Body.AsNumber(v) ?? double.NaN;
                    if (double.IsNaN(n) || n < 0 || n > 100) throw HttpException.BadRequest($"{label} must be between 0 and 100, or empty");
                    return JsNumber(n);
                }
                return new JsonObject
                {
                    ["minOverallPercent"] = Limit("minOverallPercent", "Minimum overall percentage"),
                    ["minAttendancePercent"] = Limit("minAttendancePercent", "Minimum attendance"),
                    ["requirePass"] = p["requirePass"] is JsonValue r && r.TryGetValue<bool>(out var rp) && rp,
                };
            }
            case "report_card":
            {
                var title = (Body.AsString(p["title"]) ?? "").Trim();
                if (title == "" || title.Length > 80) throw HttpException.BadRequest("Give the report card a title of up to 80 characters");
                bool NotFalse(string key) => !(p[key] is JsonValue v && v.TryGetValue<bool>(out var b) && !b);
                var note = (Body.AsString(p["footerNote"]) ?? "").Trim();
                return new JsonObject
                {
                    ["title"] = title,
                    ["showPercentage"] = NotFalse("showPercentage"),
                    ["showGrade"] = NotFalse("showGrade"),
                    ["showRank"] = NotFalse("showRank"),
                    ["showAttendance"] = NotFalse("showAttendance"),
                    ["showAffiliation"] = NotFalse("showAffiliation"),
                    ["footerNote"] = note.Length > 300 ? note[..300] : note,
                };
            }
            case "bell_schedule":
                if (payload is not JsonArray { Count: > 0 } list) throw HttpException.BadRequest("Send at least one period");
                return list.DeepClone();
            default:
                throw HttpException.BadRequest("Unknown rule type");
        }
    }

    /// <summary>A whole number as an integer node (so it prints as 40, not 40.0).</summary>
    private static JsonNode JsNumber(double n) => n == Math.Floor(n) && Math.Abs(n) < 1e15 ? JsonValue.Create((long)n) : JsonValue.Create(n);

}