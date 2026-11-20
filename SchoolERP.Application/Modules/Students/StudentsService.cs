using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Modules.Admissions;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Modules.Fees;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Students;

/// <summary>Students (the Node app's academics/students.routes.js, students.service.js and students.validation.js).</summary>
public sealed partial class StudentsService(
    IApplicationDbContext db,
    SettingsReader settings,
    RecordExtras extras,
    IBoardService boards,
    FeeBilling feeBilling,
    ISubjectMapService subjectMap,
    IFileStorage files)
{
    private const int MaxPageSize = 100;
    private static readonly string[] Genders = ["male", "female", "other"];
    private static readonly string[] Statuses = ["active", "inactive"];
    private static readonly string[] RegistrationStatuses = ["complete", "pending", "under_review"];
    private static readonly string[] Sources = ["online", "office", "phone", "referral", "social", "other"];

    /// <summary>The photo and signature kinds stored per student; removed with the student.</summary>
    public static readonly string[] PhotoEntities =
        ["student", "student_sign", "father_photo", "father_sign", "mother_photo", "mother_sign", "student_thumb"];

    public sealed record ListQuery(
        string? Q, string? ClassId, string? Status, string? RegistrationStatus, string? Grade, string? Section, string? Page, string? PageSize);

    // ---------- validation ----------

    /// <summary>Problems with a student payload; empty when valid. <paramref name="partial"/> skips the required checks.</summary>
    public async Task<List<string>> ValidateAsync(Body body, bool partial, bool admissionNoAuto = false, CancellationToken ct = default)
    {
        var errors = new List<string>();
        foreach (var (field, label) in new[] { ("admissionNo", "Admission no"), ("firstName", "First name"), ("lastName", "Last name") })
        {
            if (partial) continue;
            if (field == "admissionNo" && admissionNoAuto && body.IsBlank(field)) continue; // generated
            if (body.IsBlank(field) || body.Trimmed(field) == "") errors.Add($"{label} is required");
        }

        if (!body.IsBlank("gender") && !Genders.Contains(body.Str("gender"))) errors.Add("Gender is invalid");
        if (!body.IsBlank("status") && !Statuses.Contains(body.Str("status"))) errors.Add("Status is invalid");
        if (!body.IsBlank("registrationStatus") && !RegistrationStatuses.Contains(body.Str("registrationStatus")))
            errors.Add("Registration status must be complete, pending or under_review");

        if (!body.IsBlank("dob"))
        {
            var dob = body.Date("dob");
            if (dob is null) errors.Add("Date of birth is invalid");
            else if (dob > Clock.Today()) errors.Add("Date of birth cannot be in the future");
        }
        if (!body.IsBlank("guardianPhone") && !Phone().IsMatch(body.Str("guardianPhone")!)) errors.Add("Guardian phone is invalid");

        if (!body.IsBlank("classId"))
        {
            var classId = body.Int("classId");
            if (classId is null || !await db.Set<SchoolClass>().AnyAsync(c => c.Id == classId, ct)) errors.Add("Class does not exist");
        }

        // registration details: lengths first (a clear message rather than a database error), then formats
        foreach (var f in StudentFields.Details)
            if (f.Max > 0 && !body.IsBlank(f.Key) && body.Str(f.Key)!.Length > f.Max)
                errors.Add($"{f.Label} must be {f.Max} characters or fewer");
        foreach (var (key, label) in new[] { ("email", "Student email"), ("guardianEmail", "Guardian email") })
            if (!body.IsBlank(key) && !Email().IsMatch(body.Str(key)!)) errors.Add($"{label} is invalid");
        foreach (var (key, label) in new[] { ("phone", "Student phone"), ("fatherPhone", "Father's phone"), ("motherPhone", "Mother's phone"), ("secondaryGuardianPhone", "Secondary contact") })
            if (!body.IsBlank(key) && !Phone().IsMatch(body.Str(key)!)) errors.Add($"{label} is invalid");
        if (!body.IsBlank("pincode") && !Pincode().IsMatch(body.Str("pincode")!)) errors.Add("PIN / postal code is invalid");
        if (!body.IsBlank("nationalId") && !NationalId().IsMatch(body.Str("nationalId")!))
            errors.Add("Aadhaar / national ID must be 4 to 20 letters, digits or dashes");
        if (!body.IsBlank("admissionDate"))
        {
            var d = body.Date("admissionDate");
            if (d is null) errors.Add("Date of admission is invalid");
            else if (d > Clock.Today()) errors.Add("Date of admission cannot be in the future");
        }
        if (!body.IsBlank("previousPercent") && !(body.Num("previousPercent") is { } p && p >= 0 && p <= 100))
            errors.Add("Last result must be between 0 and 100");
        return errors;
    }

    // ---------- shaping ----------

    private static string Day(DateOnly? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";

    /// <summary>The student's columns plus the class as a "Class-Section" label, the class teacher, and the display ids.</summary>
    private static JsonObject Shape(Student student)
    {
        var cls = student.SchoolClass;
        student.SchoolClass = null;
        var json = JsonDefaults.ToJsonObject(student);
        student.SchoolClass = cls;
        var registrationDate = Day(student.AdmissionDate) is { Length: > 0 } a ? a : student.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var year = registrationDate.Length >= 4 ? registrationDate[..4] : DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
        var number = student.Id.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0');
        json["className"] = cls is null ? null : $"{cls.Name}-{cls.Section}";
        json["classTeacherName"] = cls?.ClassTeacher is { } t ? $"{t.FirstName} {t.LastName}" : null;
        json["classTeacherPhone"] = cls?.ClassTeacher?.Phone;
        json["studentId"] = $"STU-{year}-{number}";
        json["registrationId"] = $"REG-{year}-{number}";
        json["registrationDate"] = registrationDate;
        return json;
    }

    private IQueryable<Student> WithClass() =>
        db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).ThenInclude(c => c!.ClassTeacher);

    private async Task<HashSet<int>> PhotoIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct) =>
        ids.Count == 0
            ? []
            : (await db.Set<Photo>().Where(p => p.Entity == "student" && ids.Contains(p.EntityId)).Select(p => p.EntityId).ToListAsync(ct)).ToHashSet();

    // ---------- reads ----------

    /// <summary>
    /// Students, newest first. Filters: q (name, admission no, roll no, phones, STU-/REG- ids), classId, status, registrationStatus,
    /// grade (class name), section. Without page the full list is returned (used by dropdowns); with page, { rows, total, page, pageSize }.
    /// </summary>
    public async Task<object> ListAsync(ListQuery query, CancellationToken ct = default)
    {
        var students = WithClass();
        if (!string.IsNullOrEmpty(query.Grade)) students = students.Where(s => s.SchoolClass != null && s.SchoolClass.Name == query.Grade);
        if (!string.IsNullOrEmpty(query.Section)) students = students.Where(s => s.SchoolClass != null && s.SchoolClass.Section == query.Section);
        if (!string.IsNullOrEmpty(query.RegistrationStatus)) students = students.Where(s => s.RegistrationStatus == query.RegistrationStatus);
        if (!string.IsNullOrEmpty(query.ClassId))
        {
            var classId = int.TryParse(query.ClassId, out var c) ? c : -1;
            students = students.Where(s => s.ClassId == classId);
        }
        if (!string.IsNullOrEmpty(query.Status)) students = students.Where(s => s.Status == query.Status);
        if (!string.IsNullOrEmpty(query.Q))
        {
            var like = $"%{query.Q}%";
            var q = query.Q.Trim();
            var idMatch = DisplayId().Match(q);
            var byId = idMatch.Success ? int.Parse(idMatch.Groups[1].Value, CultureInfo.InvariantCulture) : (int?)null;
            students = students.Where(s =>
                EF.Functions.Like(s.AdmissionNo, like) || EF.Functions.Like(s.FirstName, like) || EF.Functions.Like(s.LastName, like)
                || EF.Functions.Like(s.GuardianPhone!, like) || EF.Functions.Like(s.FatherPhone!, like) || EF.Functions.Like(s.MotherPhone!, like)
                || EF.Functions.Like(s.RollNo!, like) || EF.Functions.Like(s.GuardianName!, like) || EF.Functions.Like(s.FatherName!, like)
                || EF.Functions.Like(s.MotherName!, like) || (byId != null && s.Id == byId));
        }
        students = students.OrderByDescending(s => s.Id);

        if (string.IsNullOrEmpty(query.Page))
        {
            var all = await students.ToListAsync(ct);
            var withPhoto = await PhotoIdsAsync(all.Select(s => s.Id).ToList(), ct);
            return all.Select(s => WithPhotoFlag(Shape(s), withPhoto.Contains(s.Id))).ToList();
        }

        var size = Math.Clamp(int.TryParse(query.PageSize, out var ps) && ps != 0 ? ps : 10, 1, MaxPageSize);
        var current = Math.Max(int.TryParse(query.Page, out var pg) && pg != 0 ? pg : 1, 1);
        var total = await students.CountAsync(ct);
        var rows = await students.Skip((current - 1) * size).Take(size).ToListAsync(ct);
        var photos = await PhotoIdsAsync(rows.Select(s => s.Id).ToList(), ct);
        return new
        {
            rows = rows.Select(s => WithPhotoFlag(Shape(s), photos.Contains(s.Id))).ToList(),
            total,
            page = current,
            pageSize = size,
        };
    }

    private static JsonObject WithPhotoFlag(JsonObject json, bool hasPhoto)
    {
        json["hasPhoto"] = hasPhoto;
        return json;
    }

    public async Task<JsonObject?> GetAsync(int id, CancellationToken ct = default)
    {
        var student = await WithClass().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null) return null;
        var json = Shape(student);
        foreach (var (key, value) in await extras.LoadAsync("student", student.Id, ct)) json[key] = value?.DeepClone();
        return json;
    }

    // ---------- writes ----------

    /// <summary>The personal, parent, contact, address, qualification and official details the registration form sends, checked and cleaned.</summary>
    private static JsonObject ExtrasOf(Body body)
    {
        var (data, error) = EnquiryFields.Clean(body);
        if (error is not null) throw HttpException.BadRequest(error);
        var result = new JsonObject();
        foreach (var (key, value) in data)
            if (EnquiryFields.StudentExtraKeys.Contains(key)) result[key] = value?.DeepClone();
        if (body.Has("source"))
        {
            var source = body.Trimmed("source");
            if (source != "" && !Sources.Contains(source)) throw HttpException.BadRequest("Source is invalid");
            result["source"] = source == "" ? null : source;
        }
        if (body.Has("referredBy"))
        {
            var referredBy = body.Trimmed("referredBy");
            if (referredBy.Length > 100) throw HttpException.BadRequest("Referenced by must be 100 characters or fewer");
            result["referredBy"] = referredBy == "" ? null : referredBy;
        }
        return result;
    }

    private static readonly Dictionary<string, PropertyInfo> Properties = StudentFields.Writable.ToDictionary(
        key => key,
        key => typeof(Student).GetProperty(char.ToUpperInvariant(key[0]) + key[1..]) ?? throw new InvalidOperationException($"Student has no {key}"));

    /// <summary>Copies the whitelisted fields that were sent; blank values become NULL so optional columns are cleared.</summary>
    private static void Apply(Body body, Student student)
    {
        foreach (var key in StudentFields.Writable)
        {
            if (!body.Has(key)) continue;
            if (key == "registrationStatus" && body.IsBlank(key)) continue; // never blank: the column has a default
            var prop = Properties[key];
            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (body.IsBlank(key))
            {
                prop.SetValue(student, null);
                continue;
            }
            object? value = type == typeof(string) ? body.Str(key)
                : type == typeof(int) ? body.Int(key)
                : type == typeof(decimal) ? (decimal?)body.Num(key)
                : type == typeof(DateOnly) ? body.Date(key)
                : throw new InvalidOperationException($"Unhandled type for {key}");
            prop.SetValue(student, value);
        }
    }

    private async Task<string> NextAdmissionNoAsync(CancellationToken ct)
    {
        var formula = await settings.GetStringAsync("numbering.admission", ct);
        return await Numbering.NextAsync(
            formula,
            async () => await db.Set<Student>().MaxAsync(s => (int?)s.Id, ct),
            async candidate => await db.Set<Student>().AnyAsync(s => s.AdmissionNo == candidate, ct));
    }

    /// <summary>A student with the same name and date of birth is almost always a double entry.</summary>
    private async Task<Student?> FindDuplicateAsync(Body body, CancellationToken ct)
    {
        var dob = body.Date("dob");
        if (dob is null || body.IsBlank("firstName") || body.IsBlank("lastName")) return null;
        var first = body.Trimmed("firstName");
        var last = body.Trimmed("lastName");
        return await db.Set<Student>().AsNoTracking().FirstOrDefaultAsync(s => s.FirstName == first && s.LastName == last && s.Dob == dob, ct);
    }

    /// <summary>
    /// Registers a student. With billFees the class's fee structure is billed in the same transaction, so a student is never
    /// left registered but half billed. Returns { id, admissionNo, feesBilled }.
    /// </summary>
    public async Task<object> CreateAsync(Body body, bool allowDuplicate, bool billFees, CancellationToken ct = default)
    {
        if (!allowDuplicate && await FindDuplicateAsync(body, ct) is { } twin)
            throw HttpException.Conflict(
                $"A student with the same name and date of birth already exists ({twin.AdmissionNo}). Save again to register them anyway.");

        return await db.InTransactionAsync<object>(async () =>
        {
            var extraValues = ExtrasOf(body);
            var student = new Student();
            Apply(body, student);
            if (string.IsNullOrEmpty(student.AdmissionNo)) student.AdmissionNo = await NextAdmissionNoAsync(ct);
            db.Set<Student>().Add(student);
            await db.SaveChangesAsync(ct);
            await extras.SaveAsync("student", student.Id, extraValues, ct);
            var feesBilled = 0;
            if (billFees && student.ClassId is { } classId)
            {
                var items = await db.Set<FeeStructure>().AsNoTracking().Where(f => f.ClassId == classId).ToListAsync(ct);
                foreach (var item in items) await feeBilling.BillStudentsAsync([student.Id], item.Title, item.Amount, item.DueDate, ct);
                feesBilled = items.Count;
            }
            return new { id = student.Id, admissionNo = student.AdmissionNo, feesBilled };
        }, ct);
    }

    /// <summary>Returns false when the student does not exist.</summary>
    public async Task<bool> UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var student = await db.Set<Student>().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is null) return false;
        var before = student.ClassId;
        var extraValues = ExtrasOf(body);
        if (body.Has("classId") && !body.IsBlank("classId") && body.Int("classId") is { } next && before is not null && next != before)
            await boards.AssertSameBoardAsync(before, next, body.Raw("allowCrossBoard") is JsonValue v && v.TryGetValue<bool>(out var b) && b, ct);
        Apply(body, student);
        await db.SaveChangesAsync(ct);
        await extras.SaveAsync("student", student.Id, extraValues, ct);
        if (student.ClassId != before) await subjectMap.PruneElectivesAsync([student.Id], ct);
        return true;
    }

    /// <summary>Soft-deletes a student with their records, files, login and photos. Returns false when the student does not exist.</summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (await db.Set<DocumentRequest>().AnyAsync(d => d.StudentId == id, ct))
            throw HttpException.Conflict("This student has certificate requests on record. Mark the student inactive instead of deleting.");
        var stored = await db.Set<StudentFile>().Where(f => f.StudentId == id).Select(f => f.StoredName).ToListAsync(ct);
        var login = await db.Set<StudentAccount>().AsNoTracking().FirstOrDefaultAsync(a => a.StudentId == id, ct);
        var student = await db.Set<Student>().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (student is not null)
        {
            db.Set<Student>().Remove(student);
            await db.SaveChangesAsync(ct);
            await files.RemoveAsync(stored, ct);
            // an applicant who was enrolled as this student goes back to holding an offer, so nothing points at a student that is gone
            var enrolled = await db.Set<Admission>().Where(a => a.StudentId == id && a.Status == "enrolled").ToListAsync(ct);
            foreach (var a in enrolled) (a.Status, a.StudentId, a.EnrolledOn) = ("approved", null, null);
            await db.SaveChangesAsync(ct);
        }
        if (login is not null && await db.Set<User>().FirstOrDefaultAsync(u => u.Id == login.UserId, ct) is { } user)
        {
            db.Set<User>().Remove(user);
            await db.SaveChangesAsync(ct);
        }
        if (student is null) return false;
        var photos = await db.Set<Photo>().Where(p => PhotoEntities.Contains(p.Entity) && p.EntityId == id).ToListAsync(ct);
        db.Set<Photo>().RemoveRange(photos);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ---------- summaries ----------

    /// <summary>Tiles for the Registrations page: how many registrations are complete and how many still need review.</summary>
    public async Task<object> RegistrationSummaryAsync(CancellationToken ct = default)
    {
        var total = await db.Set<Student>().CountAsync(ct);
        var complete = await db.Set<Student>().CountAsync(s => s.RegistrationStatus == "complete", ct);
        return new { total, complete, pending = total - complete };
    }

    /// <summary>Tiles for the Students page. Pending enrollment = applicants who have an offer but are not yet students.</summary>
    public async Task<object> SummaryAsync(CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
        var total = await db.Set<Student>().CountAsync(ct);
        var active = await db.Set<Student>().CountAsync(s => s.Status == "active", ct);
        var addedThisMonth = await db.Set<Student>().CountAsync(s => s.CreatedAt >= monthStart, ct);
        var pendingEnrollment = await db.Set<Admission>().CountAsync(a => a.Status == "approved", ct);
        var classes = await db.Set<SchoolClass>().CountAsync(ct);
        return new { total, active, inactive = total - active, addedThisMonth, pendingEnrollment, classes };
    }

    [GeneratedRegex(@"^[0-9+\-\s()]{7,20}$")]
    private static partial Regex Phone();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex Email();

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9 -]{2,11}$")]
    private static partial Regex Pincode();

    [GeneratedRegex(@"^[A-Za-z0-9-]{4,20}$")]
    private static partial Regex NationalId();

    [GeneratedRegex(@"^(?:stu|reg)-?\d{4}-?(\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex DisplayId();
}
