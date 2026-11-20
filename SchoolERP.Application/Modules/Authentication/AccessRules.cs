using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;

namespace SchoolERP.Application.Modules.Authentication;

/// <summary>
/// Custom roles (the Node app's platform/access.js). A custom role is a built-in role (its base) narrowed to chosen modules, so it can
/// never do more than the base role could. Each module is a sidebar group; its API prefixes are what the server checks.
/// </summary>
public static class AccessRules
{
    public sealed record Module(string Key, string Label, string[] Prefixes);

    public static readonly IReadOnlyList<Module> Modules =
    [
        new("board-management", "Board Management", ["boards", "board-registration"]),
        new("admissions", "Admissions", ["admissions", "visitors"]),
        new("students", "Students", ["students", "student-files", "student-health", "student-profile", "behavior", "alumni", "student-accounts", "promotion"]),
        new("academics", "Academics", ["classes", "my-class", "subjects", "class-subjects", "academic-years", "homework", "online-classes", "syllabus", "study-materials", "calendar", "lms"]),
        new("attendance", "Attendance", ["attendance", "period-attendance", "student-leaves", "absence-alerts", "biometric"]),
        new("examinations", "Examinations", ["exams", "progress-card"]),
        new("fees-and-finance", "Fees & Finance", ["fees", "fee-structures", "fee-groups", "accounts"]),
        new("staff-and-hr", "Staff & HR", ["staff", "hr", "payroll", "recruitment", "performance-reviews", "staff-files"]),
        new("timetable", "Timetable", ["timetable", "scheduling"]),
        new("communication", "Communication", ["messaging", "notices", "feedback"]),
        new("parent-portal", "Parent Portal", ["parents"]),
        new("transport", "Transport", ["transport"]),
        new("library", "Library", ["library"]),
        new("inventory", "Inventory", ["inventory", "inventory-requests"]),
        new("hostel", "Hostel", ["hostel"]),
        new("health-and-safety", "Health & Safety", ["safety"]),
        new("events-and-activities", "Events & Activities", ["activities"]),
        new("documents-and-certificates", "Documents & Certificates", ["documents", "document-checks"]),
        new("reports", "Reports", ["reports", "report-designer"]),
        new("settings", "Settings", ["settings", "branches", "masters", "setup", "leave-types"]),
    ];
    // Not grantable: users, roles, user groups, audit log and login history, data (backup, restore, export, import) and integrations.

    public static readonly string[] Levels = ["read", "write"];
    public static readonly string[] BaseRoles = ["admin", "teacher", "accountant"];

    private static readonly Dictionary<string, string> PhotoModule = new()
    {
        ["student"] = "students",
        ["staff"] = "staff-and-hr",
        ["enquiry"] = "admissions",
        ["adm_student_sign"] = "admissions",
        ["adm_father_photo"] = "admissions",
        ["adm_father_sign"] = "admissions",
        ["adm_mother_photo"] = "admissions",
        ["adm_mother_sign"] = "admissions",
        ["adm_student_thumb"] = "admissions",
        ["school"] = "settings",
    };

    private static readonly Dictionary<string, string> ModuleOf =
        Modules.SelectMany(m => m.Prefixes.Select(p => (p, m.Key))).ToDictionary(x => x.p, x => x.Key);

    /// <summary>Read-only lookups other screens need (dropdowns, the dashboard), available to every custom role.</summary>
    private static readonly HashSet<string> SharedReads =
    [
        "dashboard", "classes", "subjects", "academic-years", "students", "staff", "settings", "notices", "calendar", "branches",
        "masters", "lookup", "leave-types",
    ];

    /// <summary>Help and support belong to every signed-in staff member, whatever a custom role includes.</summary>
    private static readonly HashSet<string> Always = ["auth", "support", "push", "feedback"];

    public static bool IsRead(string method) => method is "GET" or "HEAD" or "OPTIONS";

    private static string[] Parts(string urlPath) => urlPath.Split('?')[0].Split('/', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Turns what the form sends into a clean { moduleKey: "read" | "write" } map, dropping "none" and anything unknown.</summary>
    public static JsonObject CleanPermissions(JsonObject? input)
    {
        var output = new JsonObject();
        foreach (var m in Modules)
            if (Body.AsString(input?[m.Key]) is { } level && Levels.Contains(level)) output[m.Key] = level;
        return output;
    }

    /// <summary>Whether a custom role's permission map allows this request. Anything not named in <see cref="Modules"/> is refused.</summary>
    public static bool Permits(JsonObject permissions, string method, string urlPath)
    {
        var parts = Parts(urlPath); // ["api", "fees", ...]
        if (parts.Length < 2) return false;
        var prefix = parts[1];
        if (Always.Contains(prefix)) return true;
        var module = prefix == "photos"
            ? (parts.Length > 2 ? PhotoModule.GetValueOrDefault(parts[2]) : null)
            : ModuleOf.GetValueOrDefault(prefix);
        var level = module is null ? null : Body.AsString(permissions[module]);
        if (prefix == "photos" && IsRead(method) && parts.Length > 2 && parts[2] == "school") return true;
        if (IsRead(method)) return level is not null || SharedReads.Contains(prefix);
        return level == "write";
    }

    /// <summary>The module an API address belongs to (/api/fees/3 is fees-and-finance), or null when it belongs to none.</summary>
    public static string? ModuleOfPath(string urlPath)
    {
        var parts = Parts(urlPath);
        if (parts.Length < 2) return null;
        return parts[1] == "photos"
            ? (parts.Length > 2 ? PhotoModule.GetValueOrDefault(parts[2]) : null)
            : ModuleOf.GetValueOrDefault(parts[1]);
    }
}
