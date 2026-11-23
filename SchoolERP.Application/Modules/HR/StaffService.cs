using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.HR;

/// <summary>
/// Staff records (the Node app's hr/staff.routes.js, staffFields.js, staffNumber.js and profileMine.js). Any staff role reads,
/// only an admin changes; what a reader sees depends on their role.
/// </summary>
public sealed partial class StaffService(IStaffRepository staffRepo, IPayslipRepository payslips, IUnitOfWork unitOfWork, ISettingsReader settings) : IStaffService
{
    /// <summary>
    /// A detail kept on a staff record. See: null = any staff role, "finance" = admin and accountant (payroll needs bank details),
    /// "admin" = admin only (personal and identity data).
    /// </summary>
    public sealed record Detail(string Key, string Label, string Type, int? Max = null, string[]? OneOf = null, string? See = null);

    public static readonly IReadOnlyList<Detail> Details =
    [
        new("staffCategory", "Staff category", "text", 20, ["teaching", "non_teaching"]),
        new("middleName", "Middle name", "text", 80),
        new("gender", "Gender", "text", 10, ["male", "female", "other"]),
        new("dob", "Date of birth", "date", See: "admin"),
        new("maritalStatus", "Marital status", "text", 20, See: "admin"),
        new("bloodGroup", "Blood group", "text", 5, ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"], "admin"),
        new("nationalId", "Aadhaar / national ID no", "text", 20, See: "admin"),
        new("panNo", "PAN / tax ID", "text", 20, See: "admin"),
        new("address", "Address", "text", 255, See: "admin"),
        new("city", "City / town", "text", 80, See: "admin"),
        new("state", "State", "text", 80, See: "admin"),
        new("pincode", "PIN / postal code", "text", 12, See: "admin"),
        new("qualification", "Highest qualification", "text", 120),
        new("experienceYears", "Years of experience", "number"),
        new("employmentType", "Employment type", "text", 20, ["permanent", "contract", "part_time", "visiting"]),
        new("previousEmployer", "Previous employer", "text", 160, See: "admin"),
        new("emergencyName", "Emergency contact", "text", 100, See: "admin"),
        new("emergencyPhone", "Emergency phone", "text", 20, See: "admin"),
        new("bankName", "Bank name", "text", 100, See: "finance"),
        new("bankAccountNo", "Bank account number", "text", 30, See: "finance"),
        new("ifsc", "IFSC / branch code", "text", 20, See: "finance"),
        new("remarks", "Remarks", "text", 500, See: "admin"),
    ];

    private static readonly string[] DetailKeys = Details.Select(d => d.Key).ToArray();
    private static readonly string[] Core = ["staffNo", "firstName", "lastName", "designation", "departmentId", "phone", "email", "joinDate", "status"];
    private static readonly string[] Writable = [.. Core, .. DetailKeys];

    /// <summary>The detail keys a role may read.</summary>
    public static HashSet<string> VisibleDetailKeys(string? role) => Details
        .Where(d => d.See is null || (d.See == "finance" ? role is "admin" or "accountant" : role == "admin"))
        .Select(d => d.Key).ToHashSet();

    /// <summary>A staff row as a reader with this role may see it, plus departmentName.</summary>
    public static JsonObject Shape(Staff staff, string? role)
    {
        var row = JsonDefaults.ToJsonObject(staff);
        row.Remove("Department");
        var visible = VisibleDetailKeys(role);
        foreach (var key in DetailKeys.Where(k => !visible.Contains(k))) row.Remove(key);
        row["departmentName"] = staff.Department?.Name;
        return row;
    }

    private static bool Blank(Body b, string key) => b.IsBlank(key);

    private static bool ValidDate(string? text) =>
        DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out _);

    /// <summary>Problems with a staff payload; empty when valid (also used by the bulk staff import).</summary>
    public static List<string> Problems(Body b, bool partial = false)
    {
        var errors = new List<string>();
        foreach (var (key, label) in new[] { ("firstName", "First name"), ("lastName", "Last name") })
            // String(null) is "null" in JavaScript, so only a sent text that trims to nothing counts as blank on an update
            if ((!partial && Blank(b, key)) || (b.Has(key) && b.Raw(key) is not null && b.Trimmed(key) == ""))
                errors.Add($"{label} is required");
        foreach (var f in Details)
        {
            if (Blank(b, f.Key)) continue;
            var v = b.Str(f.Key) ?? "";
            if (f.Max is { } max && v.Length > max) errors.Add($"{f.Label} must be {max} characters or fewer");
            if (f.OneOf is { } oneOf && !oneOf.Contains(v)) errors.Add($"{f.Label} is invalid");
        }
        if (!Blank(b, "email") && !Email().IsMatch(b.Str("email")!)) errors.Add("Email is invalid");
        foreach (var (key, label) in new[] { ("phone", "Phone"), ("emergencyPhone", "Emergency phone") })
            if (!Blank(b, key) && !Phone().IsMatch(b.Str(key)!)) errors.Add($"{label} is invalid");
        if (!Blank(b, "pincode") && !Pincode().IsMatch(b.Str("pincode")!)) errors.Add("PIN / postal code is invalid");
        if (!Blank(b, "nationalId") && !NationalId().IsMatch(b.Str("nationalId")!))
            errors.Add("Aadhaar / national ID must be 4 to 20 letters, digits or dashes");
        if (!Blank(b, "bankAccountNo") && !BankAccount().IsMatch(b.Str("bankAccountNo")!)) errors.Add("Bank account number is invalid");
        foreach (var (key, label) in new[] { ("dob", "Date of birth"), ("joinDate", "Joining date") })
            if (!Blank(b, key) && !ValidDate(b.Str(key))) errors.Add($"{label} is invalid");
        if (!Blank(b, "dob") && ValidDate(b.Str("dob")) && DateTime.Parse(b.Str("dob")!, CultureInfo.InvariantCulture) > DateTime.Now)
            errors.Add("Date of birth cannot be in the future");
        if (!Blank(b, "experienceYears") && !(b.Num("experienceYears") is { } y && y >= 0 && y <= 80))
            errors.Add("Years of experience must be between 0 and 80");
        if (!Blank(b, "status") && b.Str("status") is not ("active" or "inactive")) errors.Add("Status is invalid");
        if (!Blank(b, "departmentId") && !(b.Num("departmentId") is { } d && d == Math.Floor(d))) errors.Add("Department is invalid");
        return errors;
    }

    /// <summary>The next staff number from the school's formula (default STF-2026-0007); numbers already taken are skipped.</summary>
    public async Task<string> NextStaffNoAsync(CancellationToken ct = default) =>
        await Numbering.NextAsync(
            await settings.GetStringAsync("numbering.staff", ct),
            async () => await staffRepo.MaxIdIncludingDeletedAsync(ct),
            async candidate => await staffRepo.StaffNoTakenAsync(candidate, ct));

    public async Task<List<JsonObject>> ListAsync(string? role, CancellationToken ct = default) =>
        (await staffRepo.ListWithDepartmentAsync(ct))
        .Select(s => Shape(s, role)).ToList();

    public async Task<JsonObject> GetAsync(int id, string? role, CancellationToken ct = default) =>
        Shape(await staffRepo.GetWithDepartmentAsync(id, ct)
            ?? throw HttpException.NotFound("Staff member not found"), role);

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        var errors = Problems(body);
        if (errors.Count > 0) throw HttpException.BadRequest(errors[0]);
        var staff = new Staff();
        CrudService.Apply(body, Writable, staff);
        if (string.IsNullOrEmpty(staff.StaffNo)) staff.StaffNo = await NextStaffNoAsync(ct);
        staffRepo.Add(staff);
        await unitOfWork.SaveChangesAsync(ct);
        return staff.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var errors = Problems(body, partial: true);
        if (errors.Count > 0) throw HttpException.BadRequest(errors[0]);
        var staff = await staffRepo.FindAsync(id, ct) ?? throw HttpException.NotFound("Staff member not found");
        var keep = staff.StaffNo;
        CrudService.Apply(body, Writable, staff);
        if (string.IsNullOrEmpty(staff.StaffNo)) staff.StaffNo = keep; // the number is never blanked
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (await payslips.AnyForStaffAsync(id, ct))
            throw HttpException.Conflict("This staff member has payslips on record. Mark them inactive instead of deleting.");
        await staffRepo.RemoveWithPhotosAsync(id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static readonly (string Title, string[] Keys)[] ProfileGroups =
    [
        ("Personal", ["middleName", "gender", "dob", "maritalStatus", "bloodGroup"]),
        ("Contact and address", ["address", "city", "state", "pincode", "emergencyName", "emergencyPhone"]),
        ("Identity", ["nationalId", "panNo"]),
        ("Qualification and experience", ["qualification", "experienceYears", "previousEmployer"]),
        ("Bank", ["bankName", "bankAccountNo", "ifsc"]),
        ("Notes", ["remarks"]),
    ];

    /// <summary>"part_time" → "Part time".</summary>
    private static string Nice(string v)
    {
        var t = v.Replace('_', ' ');
        return t.Length == 0 ? t : char.ToUpperInvariant(t[0]) + t[1..];
    }

    /// <summary>A staff member's own record laid out in sections, with every detail the school holds about them (it is their own data).</summary>
    public async Task<object?> OwnProfileAsync(int staffId, CancellationToken ct = default)
    {
        var staff = await staffRepo.GetWithDepartmentAsync(staffId, ct);
        if (staff is null) return null;
        var s = JsonDefaults.ToJsonObject(staff);
        const string none = "—";
        string Show(string key, JsonNode? v)
        {
            var text = Body.AsString(v);
            if (string.IsNullOrEmpty(text)) return none;
            if (key is "dob" or "joinDate") return text.Length > 10 ? text[..10] : text;
            if (key == "experienceYears") return (Body.AsNumber(v) ?? 0).ToString(CultureInfo.InvariantCulture);
            return Nice(text);
        }
        string Plain(string? v) => string.IsNullOrEmpty(v) ? none : v;
        var head = new List<string[]>
        {
            new[] { "Staff number", Plain(staff.StaffNo) },
            new[] { "Designation", Plain(staff.Designation) },
            new[] { "Department", Plain(staff.Department?.Name) },
            new[] { "Category", Plain(staff.StaffCategory is { Length: > 0 } c ? Nice(c) : null) },
            new[] { "Employment type", Plain(staff.EmploymentType is { Length: > 0 } e ? Nice(e) : null) },
            new[] { "Joined on", Plain(staff.JoinDate?.ToString("yyyy-MM-dd")) },
            new[] { "Status", Plain(staff.Status is { Length: > 0 } st ? Nice(st) : null) },
            new[] { "Phone", Plain(staff.Phone) },
            new[] { "Email", Plain(staff.Email) },
        };
        var labels = Details.ToDictionary(d => d.Key, d => d.Label);
        return new
        {
            name = string.Join(' ', new[] { staff.FirstName, staff.LastName }.Where(x => !string.IsNullOrEmpty(x))),
            designation = staff.Designation,
            sections = new[] { new { title = "Employment", rows = head } }
                .Concat(ProfileGroups.Select(g => new { title = g.Title, rows = g.Keys.Select(k => new[] { labels[k], Show(k, s[k]) }).ToList() })),
        };
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex Email();

    [GeneratedRegex(@"^[0-9+\-\s()]{7,20}$")]
    private static partial Regex Phone();

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9 -]{2,11}$")]
    private static partial Regex Pincode();

    [GeneratedRegex("^[A-Za-z0-9-]{4,20}$")]
    private static partial Regex NationalId();

    [GeneratedRegex("^[0-9A-Za-z-]{6,30}$")]
    private static partial Regex BankAccount();
}
