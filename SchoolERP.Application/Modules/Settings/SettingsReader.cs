using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Settings;

/// <summary>
/// One school-wide setting: its group, label, type (text | integer | number), limits and default. A text setting may list
/// options (it must then be one of them, or blank when not required), a pattern (checked when something is entered) and required.
/// </summary>
public sealed record SettingDefinition(
    string Key, string Group, string Label, string Type, Func<JsonNode?> Default,
    double? Min = null, double? Max = null, string[]? Options = null, bool Required = false,
    Regex? Pattern = null, string? PatternHelp = null, string? Help = null);

/// <summary>
/// School-wide settings (the Node app's settings/settings.service.js): a saved override from the settings table (its value
/// is JSON text), else the setting's default. The default is what the system did before settings existed, so nothing changes
/// until an admin edits a value. Overrides are read once and kept until a save or reset.
/// </summary>
public sealed partial class SettingsReader(IApplicationDbContext db, ICurrentUser user)
{
    private static SettingDefinition Text(string key, string group, string label, int max, string def,
        string[]? options = null, bool required = false, Regex? pattern = null, string? patternHelp = null, string? help = null) =>
        new(key, group, label, "text", () => def, Max: max, Options: options, Required: required, Pattern: pattern, PatternHelp: patternHelp, Help: help);

    private static SettingDefinition Integer(string key, string group, string label, int min, int max, int def, string? help = null) =>
        new(key, group, label, "integer", () => def, min, max, Help: help);

    private static SettingDefinition Number(string key, string group, string label, double min, double max, double def, string? help = null) =>
        new(key, group, label, "number", () => def, min, max, Help: help);

    private const string FormulaHelp =
        "letters, digits and dashes plus {YYYY} {YY} {MM} {TYPE} and a required {SEQ:4} (running number), e.g. ADM-{YYYY}-{SEQ:4}";

    private static SettingDefinition Formula(string key, string label, string def, string? help = null) =>
        Text($"numbering.{key}", "Auto numbering", label, 40, def, required: true, pattern: FormulaPattern(), patternHelp: FormulaHelp, help: help);

    public static readonly IReadOnlyList<SettingDefinition> Definitions =
    [
        new("school.name", "School profile", "School name", "text",
            () => Environment.GetEnvironmentVariable("SCHOOL_NAME") is { Length: > 0 } n ? n : "School Name",
            Max: 120, Help: "Printed on certificates, ID cards and messages."),
        Text("school.code", "School profile", "School code", 30, "", pattern: SchoolCode(), patternHelp: "letters, digits and dashes only"),
        Text("school.ownership", "School profile", "Ownership", 30, "", ["Private", "Government", "Government-aided", "Trust / society", "Other"]),
        Text("school.mode", "School profile", "Admission type", 30, "", ["Co-educational", "Boys only", "Girls only"]),
        Text("school.board", "School profile", "Board / curriculum", 40, "", ["CBSE", "UP Board", "ICSE", "ISC", "State board", "IB", "Cambridge (IGCSE)", "Other"]),
        Text("school.established", "School profile", "Established", 4, "", pattern: Year(), patternHelp: "a four-digit year such as 2008"),
        Text("school.registrationNo", "School profile", "Registration no", 60, ""),
        Text("school.address", "School profile", "Address", 255, ""),
        Text("school.website", "School profile", "Website", 150, "", pattern: Website(), patternHelp: "a web address such as www.example.edu.in"),
        Text("school.motto", "School profile", "School motto", 150, ""),
        Text("school.timezone", "School profile", "Time zone", 40, "Asia/Kolkata",
            ["Asia/Kolkata", "Asia/Dubai", "Asia/Dhaka", "Asia/Kathmandu", "Asia/Colombo", "Asia/Singapore", "Europe/London", "America/New_York", "Australia/Sydney", "UTC"],
            required: true),
        Text("school.language", "School profile", "Default language", 10, "en-IN",
            ["en-IN", "en-GB", "en-US", "hi-IN", "mr-IN", "gu-IN", "bn-IN", "ta-IN", "te-IN", "kn-IN", "ml-IN"], required: true),
        Text("school.dateFormat", "School profile", "Date format", 12, "DD/MM/YYYY", ["DD/MM/YYYY", "MM/DD/YYYY", "YYYY-MM-DD"], required: true),
        Text("school.phone", "School profile", "Phone", 30, ""),
        Text("school.email", "School profile", "Email", 120, ""),
        Text("school.principal", "School profile", "Principal / head", 100, "", help: "Shown under the signature line on certificates."),

        Text("student.defaultPassword", "Student login", "Default password for new student logins", 40, "",
            pattern: Password(), patternHelp: "at least 8 characters",
            help: "Every new student login starts with this password and the student must change it at the first sign-in. Leave blank to give each student a different random password instead."),

        Number("attendance.minPercent", "Attendance", "Minimum attendance (%)", 0, 100, 75, "Students and staff below this are highlighted in reports."),
        Integer("leave.studentMaxDays", "Attendance", "Longest student leave a parent can apply for (days)", 1, 365, 60),

        Integer("grievance.escalateAfterDays", "Feedback & grievance", "Escalate a grievance to the next level after (days without a reply)", 0, 60, 3,
            "A grievance nobody has answered moves up one level: class teacher or department head, then the school office, then the principal. 0 turns automatic escalation off."),

        Integer("library.loanDays", "Library", "Loan period (days)", 1, 90, 14),
        Integer("library.requireMembership", "Library", "Students need a library card to borrow (1 = yes, 0 = no)", 0, 1, 0,
            "With 0, only students who hold a card are limited by it."),
        Integer("library.defaultMaxBooks", "Library", "Books allowed at a time on a new card", 1, 50, 3),
        Number("library.finePerDay", "Library", "Late fine per day", 0, 10000, 2),

        Integer("notify.autoAbsence", "Notifications", "Alert parents automatically when a student is marked absent (1 = yes, 0 = no)", 0, 1, 0,
            "After the time below, the parent and student logins get a push notification for every student marked absent today, except those on approved leave or already contacted."),
        Text("notify.autoAbsenceTime", "Notifications", "Send automatic absence alerts after (HH:MM)", 5, "10:00",
            pattern: ClockTime(), patternHelp: "a time like 10:00", help: "Gives teachers time to finish and correct attendance first."),
        Integer("notify.autoAbsenceSms", "Notifications", "Also text the guardian (1 = yes, 0 = no)", 0, 1, 1,
            "Uses the SMS provider under Integrations. Without one, the message is only recorded, not delivered."),
        Text("notify.absenceTemplate", "Notifications", "Absence SMS wording", 500,
            "Dear {{guardian}}, {{student}} ({{class}}) was marked absent on {{date}}. Please contact the school if this is unexpected. - {{school}}",
            help: "Placeholders: {{guardian}} {{student}} {{class}} {{date}} {{school}}."),
        Text("notify.feeReminderTemplate", "Notifications", "Fee reminder SMS wording", 500,
            "Dear {{guardian}}, fees of {{amount}} for {{student}} are overdue ({{items}}). Please pay at the school office. - {{school}}",
            help: "Placeholders: {{guardian}} {{student}} {{class}} {{amount}} {{items}} {{school}}."),
        Integer("notify.reminderCooldownHours", "Notifications", "Hours before the same family can be reminded again", 1, 720, 24),

        Text("payment.receiptPrefix", "Payments", "Receipt number prefix", 8, "RCT", required: true,
            pattern: ReceiptPrefix(), patternHelp: "letters, digits and dashes only",
            help: "New receipts are numbered like RCT-000123. Receipts already issued keep their numbers."),
        Formula("admission", "Admission number formula", "ADM-{YYYY}-{SEQ:4}", "Applies to new admission numbers only; existing numbers do not change."),
        Formula("staff", "Staff number formula", "STF-{YYYY}-{SEQ:4}"),
        Formula("library", "Library card number formula", "LIB-{SEQ:5}"),
        Formula("grievance", "Grievance ticket number formula", "FG-{YYYY}-{SEQ:4}"),
        Formula("certificate", "Certificate number formula", "{TYPE}-{YYYY}-{SEQ:5}", "{TYPE} becomes BON, TC, CHR or FEE."),
        Integer("payment.allowPartial", "Payments", "Allow part payments (1 = yes, 0 = pay in full only)", 0, 1, 1),
        Integer("payment.allowCash", "Payments", "Accept cash (1 = yes, 0 = no)", 0, 1, 1),
        Integer("payment.allowCard", "Payments", "Accept card (1 = yes, 0 = no)", 0, 1, 1),
        Integer("payment.allowUpi", "Payments", "Accept UPI (1 = yes, 0 = no)", 0, 1, 1),
        Integer("payment.allowBank", "Payments", "Accept bank transfer (1 = yes, 0 = no)", 0, 1, 1),
    ];

    private static readonly Dictionary<string, SettingDefinition> ByKey = Definitions.ToDictionary(d => d.Key);

    private static Dictionary<string, JsonNode?>? _cache;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static void Invalidate() => _cache = null;

    public async Task<JsonNode?> GetAsync(string key, CancellationToken ct = default)
    {
        if (!ByKey.TryGetValue(key, out var def)) throw new InvalidOperationException($"Unknown setting {key}");
        var saved = await OverridesAsync(ct);
        return saved.TryGetValue(key, out var value) ? value?.DeepClone() : def.Default();
    }

    public async Task<string> GetStringAsync(string key, CancellationToken ct = default) => Body.AsString(await GetAsync(key, ct)) ?? "";

    /// <summary>A numeric setting (Number(value)); 0 when it is not a number.</summary>
    public async Task<double> GetNumberAsync(string key, CancellationToken ct = default) => Body.AsNumber(await GetAsync(key, ct)) ?? 0;

    /// <summary>A 1/0 switch read with JavaScript truthiness.</summary>
    public async Task<bool> GetFlagAsync(string key, CancellationToken ct = default) =>
        new Body(new JsonObject { ["v"] = await GetAsync(key, ct) }).Truthy("v");

    /// <summary>Every setting with its current value, default and whether it has been changed, for the settings page.</summary>
    public async Task<List<object>> DescribeAllAsync(CancellationToken ct = default)
    {
        var saved = await OverridesAsync(ct);
        return Definitions.Select(d => (object)new
        {
            key = d.Key,
            group = d.Group,
            label = d.Label,
            type = d.Type,
            help = d.Help,
            min = d.Min,
            max = d.Max,
            options = d.Options,
            required = d.Required,
            @default = d.Default(),
            value = saved.TryGetValue(d.Key, out var v) ? v?.DeepClone() : d.Default(),
            changed = saved.ContainsKey(d.Key),
        }).ToList();
    }

    private static JsonNode Coerce(SettingDefinition def, JsonNode? raw)
    {
        if (def.Type == "text")
        {
            var text = (Body.AsString(raw) ?? "").Trim();
            if (text.Length > def.Max) throw HttpException.BadRequest($"{def.Label} must be {def.Max} characters or fewer");
            if (def.Required && text == "") throw HttpException.BadRequest($"{def.Label} is required");
            if (text != "" && def.Pattern is { } p && !p.IsMatch(text)) throw HttpException.BadRequest($"{def.Label}: {def.PatternHelp}");
            if (text != "" && def.Options is { } o && !o.Contains(text))
                throw HttpException.BadRequest($"{def.Label} must be one of: {string.Join(", ", o)}");
            return JsonValue.Create(text);
        }
        var blank = raw is null || (raw is JsonValue jv && jv.TryGetValue<string>(out var s) && s == "");
        var n = blank ? null : Body.AsNumber(raw);
        if (n is not { } value) throw HttpException.BadRequest($"{def.Label} must be a number");
        if (def.Type == "integer" && value != Math.Floor(value)) throw HttpException.BadRequest($"{def.Label} must be a whole number");
        if (value < def.Min || value > def.Max)
            throw HttpException.BadRequest($"{def.Label} must be between {Js(def.Min)} and {Js(def.Max)}");
        return value == Math.Floor(value) && Math.Abs(value) < 1e15 ? JsonValue.Create((long)value) : JsonValue.Create(value);
    }

    private static string Js(double? n) => n is { } v ? v.ToString(CultureInfo.InvariantCulture) : "undefined";

    /// <summary>Validates the whole batch first, then saves it, so a bad value changes nothing.</summary>
    public async Task SaveAsync(JsonObject? values, CancellationToken ct = default)
    {
        if (values is null || values.Count == 0) throw HttpException.BadRequest("Nothing to save");
        var clean = values.Select(kv =>
        {
            if (!ByKey.TryGetValue(kv.Key, out var def)) throw HttpException.BadRequest($"Unknown setting \"{kv.Key}\"");
            return (kv.Key, Value: Coerce(def, kv.Value));
        }).ToList();
        foreach (var (key, value) in clean)
        {
            var row = await db.Set<Setting>().FirstOrDefaultAsync(s => s.Key == key, ct);
            if (row is null)
            {
                row = new Setting { Key = key };
                db.Set<Setting>().Add(row);
            }
            row.Value = value.ToJsonString();
            row.UpdatedByName = user.Name;
            await db.SaveChangesAsync(ct);
        }
        Invalidate();
    }

    /// <summary>Puts a setting back to its default by removing the saved override.</summary>
    public async Task ResetAsync(string key, CancellationToken ct = default)
    {
        if (!ByKey.ContainsKey(key)) throw HttpException.BadRequest($"Unknown setting \"{key}\"");
        var rows = await db.Set<Setting>().Where(s => s.Key == key).ToListAsync(ct);
        db.Set<Setting>().RemoveRange(rows);
        await db.SaveChangesAsync(ct);
        Invalidate();
    }

    /// <summary>The school's printable identity, used on certificates, ID cards and messages.</summary>
    public async Task<SchoolProfile> SchoolProfileAsync(CancellationToken ct = default) => new(
        await GetStringAsync("school.name", ct),
        await GetStringAsync("school.address", ct),
        await GetStringAsync("school.phone", ct),
        await GetStringAsync("school.email", ct),
        await GetStringAsync("school.principal", ct));

    public sealed record SchoolProfile(string Name, string Address, string Phone, string Email, string Principal);

    /// <summary>A raw setting row's value (JSON text), also for keys outside the definitions (e.g. push.vapid).</summary>
    public async Task<string?> RawAsync(string key, CancellationToken ct = default) =>
        await db.Set<Setting>().AsNoTracking().Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);

    private async Task<Dictionary<string, JsonNode?>> OverridesAsync(CancellationToken ct)
    {
        if (_cache is { } cached) return cached;
        await Lock.WaitAsync(ct);
        try
        {
            if (_cache is { } again) return again;
            var rows = await db.Set<Setting>().AsNoTracking().ToListAsync(ct);
            var map = new Dictionary<string, JsonNode?>();
            foreach (var row in rows)
            {
                try { map[row.Key] = JsonNode.Parse(row.Value); }
                catch (System.Text.Json.JsonException) { map[row.Key] = row.Value; }
            }
            return _cache = map;
        }
        finally
        {
            Lock.Release();
        }
    }

    [GeneratedRegex("^[A-Za-z0-9-]+$")]
    private static partial Regex SchoolCode();

    [GeneratedRegex("^(1[89][0-9][0-9]|20[0-9][0-9])$")]
    private static partial Regex Year();

    [GeneratedRegex(@"^(https?://)?([A-Za-z0-9-]+\.)+[A-Za-z]{2,}(/\S*)?$")]
    private static partial Regex Website();

    [GeneratedRegex("^.{8,40}$")]
    private static partial Regex Password();

    [GeneratedRegex("^([01][0-9]|2[0-3]):[0-5][0-9]$")]
    private static partial Regex ClockTime();

    [GeneratedRegex("^[A-Za-z0-9-]{1,8}$")]
    private static partial Regex ReceiptPrefix();

    [GeneratedRegex(@"^(?=.*\{SEQ(?::[1-9])?\})(?:[A-Za-z0-9\-_./]|\{(?:YYYY|YY|MM|TYPE|SEQ(?::[1-9])?)\})+$")]
    private static partial Regex FormulaPattern();
}
