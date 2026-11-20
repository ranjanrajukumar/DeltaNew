using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Helpers;

namespace SchoolERP.Application.Modules.Admissions;

/// <summary>
/// The extra details an enquiry, an application and a student record keep beyond the registration columns (the Node app's
/// admissions/enquiryFields.js). Stored in record_extras, not as columns. type: text | bool | date | number | json.
/// </summary>
public static partial class EnquiryFields
{
    public sealed record Field(string Key, string Label, string Type, int Max = 0);

    private static Field T(string key, string label, int max) => new(key, label, "text", max);
    private static Field B(string key, string label) => new(key, label, "bool");
    private static Field D(string key, string label) => new(key, label, "date");
    private static Field N(string key, string label) => new(key, label, "number");
    private static Field J(string key, string label) => new(key, label, "json");

    public static readonly IReadOnlyList<Field> All =
    [
        // personal
        T("grandfatherName", "Grandfather's name", 120), T("nameHindi", "Student full name (Hindi)", 160),
        T("bloodGroup", "Blood group", 5), T("maritalStatus", "Marital status", 20), T("domicile", "Domicile (state)", 80),
        T("domicileNo", "Domicile no.", 40), T("passportNo", "Passport no.", 30), T("minority", "Minority", 60),
        T("minorityName", "Minority name", 80), B("physicallyHandicapped", "Physically handicapped"), T("panCard", "PAN card", 20),
        T("caste", "Caste", 60), T("casteRegNo", "Caste reg. no.", 40), T("subCaste", "Sub caste", 60),
        T("birthPlace", "Birth place", 100), T("residenceType", "Residence type", 30),
        // permanent address (address, city, state and pincode are the shared registration fields)
        T("locality", "Locality", 100), T("tehsil", "Tehsil", 80), T("thana", "Thana", 80), T("district", "District", 80),
        T("country", "Country", 60), T("landline", "Landline", 20),
        // current address
        B("sameAsPermanent", "Current address same as permanent"), T("curAddress", "Address", 255),
        T("curLocality", "Locality", 100), T("curCity", "City", 80), T("curTehsil", "Tehsil", 80), T("curThana", "Thana", 80),
        T("curDistrict", "District", 80), T("curState", "State", 80), T("curCountry", "Country", 60), T("curPincode", "Pin", 12),
        T("curLandline", "Landline", 20), T("curRemark", "Remark", 255),
        // previous qualifications: a list of rows
        J("qualifications", "Qualifications"),
        // parent details
        D("fatherDob", "Father's date of birth"), D("motherDob", "Mother's date of birth"),
        D("fatherDom", "Father's date of marriage"), D("motherDom", "Mother's date of marriage"),
        T("fatherQualification", "Father's highest qualification", 80), T("motherQualification", "Mother's highest qualification", 80),
        N("fatherQualPercent", "Father's qualification %"), N("motherQualPercent", "Mother's qualification %"),
        T("fatherDesignation", "Father's designation", 100), T("motherDesignation", "Mother's designation", 100),
        T("fatherJobType", "Father's job type", 30), T("motherJobType", "Mother's job type", 30),
        T("fatherOrgType", "Father's organisation type", 60), T("motherOrgType", "Mother's organisation type", 60),
        N("fatherIncome", "Father's annual income"), N("motherIncome", "Mother's annual income"),
        // contact details
        T("fatherEmail", "Father's email", 150), T("motherEmail", "Mother's email", 150), T("emergencyPhone", "Emergency mobile", 20),
        T("emergencyEmail", "Emergency email", 150), T("officialPhone", "Student official mobile", 20),
        T("officialEmail", "Student official email", 150), T("alternatePhone", "Alternate mobile", 20),
        T("alternateEmail", "Alternate email", 150),
        // local guardian
        T("localGuardianName", "Local guardian name", 120), T("localGuardianAddress", "Local guardian address", 255),
        T("localGuardianDesignation", "Local guardian designation", 100), T("localGuardianRelation", "Local guardian relation", 40),
        T("localGuardianPhone", "Local guardian phone", 20), T("localGuardianEmail", "Local guardian email", 150),
        // personal: the "others" flags and one more registration number
        B("otherTransport", "Transport"), B("otherHostel", "Hostel"), B("otherMess", "Mess"),
        T("incRegNo", "Income certificate reg. no.", 40),
        // local guardian: work and identity
        T("localGuardianJobType", "Local guardian job type", 30), T("localGuardianOccupation", "Local guardian occupation", 100),
        T("localGuardianDepartment", "Local guardian department", 100), T("localGuardianWorkPlace", "Local guardian place of work", 120),
        T("localGuardianLandline", "Local guardian landline", 20), T("localGuardianAadhaar", "Local guardian Aadhaar no.", 20),
        T("localGuardianPan", "Local guardian PAN card", 20),
        // admission details
        T("subStatus", "Sub status", 30), T("admThrough", "Admitted through", 30), T("feeCategory", "Fee category", 30),
        T("admUnder", "Admitted under", 40), T("serialNo", "Serial no.", 20), T("accId", "Account ID", 30),
        T("enrollNo", "Enrollment no.", 30), T("penId", "PEN ID", 30), T("apaarId", "APAAR ID", 30), D("doj", "Date of joining"),
        B("migrated", "Migration student"), B("confirmStatus", "Admission confirmed"), B("provisionalAdm", "Provisional admission"),
        T("provisionalRemark", "Provisional admission remark", 255), B("reAdmission", "Re-admission of an alumni student"),
        // brothers and sisters already in the school
        J("siblings", "Siblings"),
        // official (office use)
        T("formStatus", "Form status", 30), T("sourceSpecify", "Specify source", 100), T("subCategory", "Sub category / quota", 60),
        T("applFormNo", "Application form no.", 40), T("place", "Place", 100), B("lateralEntry", "Lateral entry"),
        B("prospectusGiven", "Prospectus given"), T("programType", "Program type", 30), B("resultAwaited", "Result awaited"),
        T("priority", "Priority", 20), T("referenceOf", "Reference of", 100),
        T("scholarshipEligibility", "Scholarship eligibility under", 100), B("physicallyVisited", "Physically visited"),
        T("studyType", "Study type", 30), T("specialization", "Specialization in", 100),
    ];

    /// <summary>Every detailed field carries over from the enquiry to the student record at enrolment.</summary>
    public static readonly IReadOnlySet<string> StudentExtraKeys = All.Select(f => f.Key).ToHashSet();

    private static readonly (string Key, string Label, int Max)[] QualificationColumns =
    [
        ("exam", "Class passed", 60), ("board", "Board", 100), ("institute", "School", 120), ("subject", "Subjects", 80),
        ("medium", "Medium", 30), ("eduStatus", "Status", 30), ("year", "Year of passing", 4), ("rollNo", "Roll no.", 30),
        ("marksObtained", "Marks obtained", 8), ("maxMarks", "Out of", 8), ("percent", "Marks %", 6), ("grade", "Division / grade", 30),
    ];

    private static readonly (string Key, string Label, int Max)[] SiblingColumns =
    [
        ("type", "Relation", 30), ("admissionNo", "Admission no. / employee code", 40), ("name", "Name", 120), ("className", "Class", 40),
    ];

    private const int MaxRows = 10;
    private static readonly string[] BloodGroups = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private static double? Number(string s) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static JsonArray ListOf(JsonNode? input)
    {
        if (input is JsonArray a) return a;
        if (input is JsonValue v && v.TryGetValue<string>(out var text) && !string.IsNullOrEmpty(text))
        {
            try { return JsonNode.Parse(text) as JsonArray ?? []; }
            catch (System.Text.Json.JsonException) { return []; }
        }
        return [];
    }

    private static (JsonArray? Rows, string? Error) CleanRows(JsonNode? input, (string Key, string Label, int Max)[] columns, bool siblings)
    {
        var rows = new JsonArray();
        foreach (var raw in ListOf(input))
        {
            var row = new Dictionary<string, string>();
            foreach (var (key, label, max) in columns)
            {
                var v = (Body.AsString(raw?[key]) ?? "").Trim();
                if (v.Length > max)
                    return (null, siblings ? $"Sibling {label.ToLowerInvariant()} must be {max} characters or fewer" : $"{label} must be {max} characters or fewer");
                row[key] = v;
            }
            var first = columns[0].Key;
            if (columns.All(c => c.Key == first || row[c.Key] == "")) continue; // only the first column: nothing recorded yet
            if (!siblings)
            {
                var name = row["exam"] != "" ? row["exam"] : "Qualification";
                if (row["year"] != "" && !(Number(row["year"]) is { } y && y >= 1950 && y <= DateTime.Now.Year + 1 && y == Math.Floor(y)))
                    return (null, $"{name}: year of passing is not valid");
                if (row["percent"] != "" && !(Number(row["percent"]) is { } p && p >= 0 && p <= 100))
                    return (null, $"{name}: marks % must be between 0 and 100");
                foreach (var key in new[] { "marksObtained", "maxMarks" })
                    if (row[key] != "" && !(Number(row[key]) is { } m && m >= 0))
                        return (null, $"{name}: marks must be a number, zero or more");
                if (row["marksObtained"] != "" && row["maxMarks"] != "" && Number(row["marksObtained"]) > Number(row["maxMarks"]))
                    return (null, $"{name}: marks obtained cannot be above the maximum");
            }
            var obj = new JsonObject();
            foreach (var (k, v) in row) obj[k] = v;
            rows.Add(obj);
        }
        if (rows.Count > MaxRows)
            return (null, siblings ? $"At most {MaxRows} siblings can be recorded" : $"At most {MaxRows} qualifications can be recorded");
        return (rows, null);
    }

    /// <summary>The extra fields of a request ready to save: text trimmed (empty = null), flags true/false. Returns the data or an error.</summary>
    public static (JsonObject Data, string? Error) Clean(Body body)
    {
        var data = new JsonObject();
        foreach (var f in All)
        {
            if (!body.Has(f.Key)) continue;
            switch (f.Type)
            {
                case "json":
                {
                    var (rows, error) = CleanRows(body.Raw(f.Key), f.Key == "siblings" ? SiblingColumns : QualificationColumns, f.Key == "siblings");
                    if (error is not null) return (data, error);
                    data[f.Key] = rows!.Count > 0 ? rows : null;
                    break;
                }
                case "bool":
                    data[f.Key] = body.IsTrue(f.Key);
                    break;
                case "date":
                {
                    if (body.IsBlankTrimmed(f.Key)) { data[f.Key] = null; break; }
                    var text = body.Trimmed(f.Key);
                    var d = text.Length >= 10 ? text[..10] : text;
                    if (!IsoDate().IsMatch(d) || !DateOnly.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        return (data, $"{f.Label} is not a valid date");
                    if (string.CompareOrdinal(d, Clock.TodayText()) > 0) return (data, $"{f.Label} cannot be in the future");
                    data[f.Key] = d;
                    break;
                }
                case "number":
                {
                    if (body.IsBlankTrimmed(f.Key)) { data[f.Key] = null; break; }
                    var n = body.Num(f.Key);
                    if (n is not { } value || double.IsInfinity(value) || value < 0) return (data, $"{f.Label} must be a number, zero or more");
                    if (f.Key.EndsWith("Percent", StringComparison.Ordinal) && value > 100) return (data, $"{f.Label} cannot be above 100");
                    if (value > 99999999999) return (data, $"{f.Label} is too large");
                    data[f.Key] = value;
                    break;
                }
                default:
                {
                    var v = body.IsBlankTrimmed(f.Key) ? null : body.Trimmed(f.Key);
                    if (v is not null && v.Length > f.Max) return (data, $"{f.Label} must be {f.Max} characters or fewer");
                    data[f.Key] = v;
                    break;
                }
            }
        }
        string? Text(string key) => data[key] is JsonValue jv && jv.TryGetValue<string>(out var s) ? s : null;
        foreach (var f in All)
        {
            var v = Text(f.Key);
            if (string.IsNullOrEmpty(v)) continue;
            if (f.Key.EndsWith("Email", StringComparison.Ordinal) && !Email().IsMatch(v)) return (data, $"{f.Label} is not a valid email address");
            if (f.Key.EndsWith("Phone", StringComparison.Ordinal) && !Phone().IsMatch(v)) return (data, $"{f.Label} must be a phone number");
        }
        if (Text("bloodGroup") is { Length: > 0 } bg && !BloodGroups.Contains(bg)) return (data, "Blood group is not valid");
        if (Text("landline") is { Length: > 0 } ll && !Phone().IsMatch(ll)) return (data, "Landline must be a phone number");
        if (Text("curLandline") is { Length: > 0 } cl && !Phone().IsMatch(cl)) return (data, "Current landline must be a phone number");
        return (data, null);
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex IsoDate();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex Email();

    [GeneratedRegex(@"^[0-9+()\-\s]{5,20}$")]
    private static partial Regex Phone();
}
