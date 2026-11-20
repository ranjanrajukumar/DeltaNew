using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Authentication;
using SchoolERP.Domain.Entities;

namespace SchoolERP.API.Middleware;

/// <summary>
/// Who may reach which part of the API: a port of the Node app's routes/index.js. Runs after <see cref="UserContextMiddleware"/>
/// has signed the user in. In order:
///  1. /api/health, /api/auth, /api/public and /api/device are open (the auth endpoints that need a user check it themselves);
///  2. every successful change is written to the audit log;
///  3. parents and students reach only /api/portal (plus push and the app menu);
///  4. each area is limited to the roles that own it: "only" = every request, "writes" = any staff may read, only these roles change.
/// Rules for single endpoints inside an area live on the controllers ([RequireRole]).
/// </summary>
public sealed partial class ApiAccessMiddleware(RequestDelegate next)
{
    private enum Kind { Only, Writes }

    private static readonly Dictionary<string, (Kind Kind, string[] Roles)> Areas = new()
    {
        ["audit"] = (Kind.Only, ["admin"]),
        ["student-accounts"] = (Kind.Only, ["admin"]),
        ["parents"] = (Kind.Only, ["admin"]),
        ["users"] = (Kind.Only, ["admin"]),
        ["global-sync"] = (Kind.Only, ["admin"]),
        ["my-class"] = (Kind.Only, ["admin", "teacher"]),
        ["classes"] = (Kind.Writes, ["admin"]),
        ["students"] = (Kind.Writes, ["admin"]),
        ["attendance"] = (Kind.Writes, ["admin", "teacher"]),
        ["academic-years"] = (Kind.Writes, ["admin"]),
        ["subjects"] = (Kind.Writes, ["admin"]),
        ["class-subjects"] = (Kind.Writes, ["admin"]),
        ["visitors"] = (Kind.Only, ["admin"]),
        ["document-checks"] = (Kind.Only, ["admin"]),
        ["report-designer"] = (Kind.Only, ["admin", "teacher", "accountant"]),
        ["boards"] = (Kind.Only, ["admin"]),
        ["board-registration"] = (Kind.Only, ["admin"]),
        ["timetable"] = (Kind.Writes, ["admin"]),
        ["promotion"] = (Kind.Only, ["admin"]),
        ["staff"] = (Kind.Writes, ["admin"]),
        ["student-health"] = (Kind.Only, ["admin", "teacher"]),
        ["behavior"] = (Kind.Only, ["admin", "teacher"]),
        ["student-profile"] = (Kind.Only, ["admin", "teacher"]),
        ["alumni"] = (Kind.Only, ["admin", "teacher"]),
        ["student-leaves"] = (Kind.Only, ["admin", "teacher"]),
        ["period-attendance"] = (Kind.Only, ["admin", "teacher"]),
        ["progress-card"] = (Kind.Only, ["admin", "teacher"]),
        ["absence-alerts"] = (Kind.Only, ["admin", "teacher"]),
        ["calendar"] = (Kind.Writes, ["admin"]),
        ["homework"] = (Kind.Writes, ["admin", "teacher"]),
        ["online-classes"] = (Kind.Writes, ["admin", "teacher"]),
        ["syllabus"] = (Kind.Writes, ["admin", "teacher"]),
        ["study-materials"] = (Kind.Writes, ["admin", "teacher"]),
        ["lms-tags"] = (Kind.Writes, ["admin"]),
        ["lms"] = (Kind.Writes, ["admin", "teacher"]),
        ["documents"] = (Kind.Only, ["admin"]),
        ["student-files"] = (Kind.Only, ["admin", "teacher"]),
        ["fee-groups"] = (Kind.Only, ["admin", "accountant"]),
        ["recruitment"] = (Kind.Only, ["admin"]),
        ["performance-reviews"] = (Kind.Only, ["admin"]),
        ["staff-files"] = (Kind.Only, ["admin"]),
        ["scheduling"] = (Kind.Only, ["admin"]),
        ["login-activity"] = (Kind.Only, ["admin"]),
        ["roles"] = (Kind.Only, ["admin"]),
        ["user-groups"] = (Kind.Only, ["admin"]),
        ["integrations"] = (Kind.Only, ["admin"]),
        ["data"] = (Kind.Only, ["admin"]),
        ["support"] = (Kind.Only, ["admin", "teacher", "accountant"]),
        ["setup"] = (Kind.Only, ["admin"]),
        ["safety"] = (Kind.Only, ["admin"]),
        ["inventory"] = (Kind.Only, ["admin"]),
        ["inventory-requests"] = (Kind.Only, ["admin", "teacher"]),
        ["payroll"] = (Kind.Only, ["admin", "accountant"]),
        ["admissions"] = (Kind.Only, ["admin"]),
        ["notices"] = (Kind.Writes, ["admin"]),
        ["library"] = (Kind.Only, ["admin"]), // the librarian is an admin-based role narrowed to this module
        ["fees"] = (Kind.Only, ["admin", "accountant"]),
        ["fee-structures"] = (Kind.Only, ["admin", "accountant"]),
        ["accounts"] = (Kind.Only, ["admin", "accountant"]),
        // set per endpoint inside: dashboard, feedback, leave-types, biometric, transport, photos, messaging, role-rights,
        // branches, masters, lookup, hostel, hr, activities, settings, exams, reports
    };

    /// <summary>A teacher reaches transport only to take bus attendance (with the routes and stops it lists).</summary>
    private static readonly string[] TeacherTransport = ["/attendance", "/routes", "/stops"];

    private const int MaxAuditDetails = 2000;

    /// <summary>HttpContext.Items key a controller sets to replace the audited request body (JSON text).</summary>
    public const string AuditDetailsKey = "AuditDetails";

    public async Task InvokeAsync(HttpContext context, ICurrentUser user, IServiceScopeFactory scopes)
    {
        var path = context.Request.ApiPath();
        if (!path.StartsWith("/api/", StringComparison.Ordinal))
        {
            await next(context);
            return;
        }
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries); // ["api", "students", "3"]
        var prefix = parts.Length > 1 ? parts[1] : "";
        if (ApiRequestExtensions.OpenAreas.Contains(prefix))
        {
            await next(context);
            return;
        }
        if (!user.IsAuthenticated) // UserContextMiddleware answers this first; kept so the rules never run without a user
        {
            await Deny(context, 401, new { error = "Unauthorized" });
            return;
        }

        await StartAuditTrailAsync(context, user, scopes, path);

        if (prefix is "push" or "app-menu")
        {
            await next(context);
            return;
        }
        // parents and students get the portal and nothing else
        if (prefix == "portal")
        {
            if (!user.IsInRole("parent", "student"))
            {
                await Deny(context, 403, new { error = "Forbidden" });
                return;
            }
            await next(context);
            return;
        }
        if (user.IsInRole("parent", "student"))
        {
            await Deny(context, 403, new { error = "Forbidden" });
            return;
        }

        if (Areas.TryGetValue(prefix, out var area))
        {
            var allowed = user.IsInRole(area.Roles) || (area.Kind == Kind.Writes && AccessRules.IsRead(context.Request.Method));
            if (!allowed)
            {
                await Deny(context, 403, new { error = "Forbidden" });
                return;
            }
        }
        if (prefix == "transport" && user.IsInRole("teacher"))
        {
            var rest = "/" + string.Join('/', parts.Skip(2));
            if (!TeacherTransport.Any(p => rest == p || rest.StartsWith(p + "/", StringComparison.Ordinal)))
            {
                await Deny(context, 403, new { error = "Forbidden" });
                return;
            }
        }

        await next(context);
    }

    private static Task Deny(HttpContext context, int status, object body)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(body, JsonDefaults.Options);
    }

    /// <summary>Reads that hand out whole tables are recorded too: a full backup and every CSV export.</summary>
    private static bool IsSensitiveRead(string path) => BackupPath().IsMatch(path) || ExportPath().IsMatch(path);

    /// <summary>
    /// Records every successful change (POST/PUT/PATCH/DELETE) and the sensitive downloads, with who did it and what was sent
    /// (secrets redacted). Written after the response; a logging failure never affects the request.
    /// </summary>
    private static async Task StartAuditTrailAsync(HttpContext context, ICurrentUser user, IServiceScopeFactory scopes, string path)
    {
        if (AccessRules.IsRead(context.Request.Method) && !IsSensitiveRead(path)) return;

        string? details = null;
        if (context.Request.ContentLength is > 0 || context.Request.Headers.TransferEncoding.Count > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var text = await reader.ReadToEndAsync(context.RequestAborted);
            context.Request.Body.Position = 0;
            details = Redacted(text);
        }

        var (userId, userName, role, method) = (user.Id, user.Name, user.Role, context.Request.Method);
        var resource = path.Split('/', StringSplitOptions.RemoveEmptyEntries) is { Length: > 1 } p ? p[1] : "unknown";
        context.Response.OnCompleted(async () =>
        {
            if (context.Response.StatusCode >= 400) return;
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                db.Set<AuditLog>().Add(new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Role = role,
                    Method = method,
                    Resource = resource,
                    Path = path,
                    Status = context.Response.StatusCode,
                    // an upload records what was uploaded (set by the controller), not megabytes of file data
                    Details = context.Items.TryGetValue(AuditDetailsKey, out var own) && own is string ownText ? ownText : details,
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                scopes.CreateScope().ServiceProvider.GetRequiredService<ILogger<ApiAccessMiddleware>>()
                    .LogError("Audit log failed: {Message}", ex.Message);
            }
        });
    }

    /// <summary>The request body with password, token and secret fields replaced, cut to 2000 characters; null when empty.</summary>
    private static string? Redacted(string text)
    {
        JsonNode? node;
        try { node = JsonNode.Parse(text); }
        catch (System.Text.Json.JsonException) { return null; }
        if (node is not JsonObject obj || obj.Count == 0) return null;
        Redact(obj);
        var json = obj.ToJsonString();
        return json.Length > MaxAuditDetails ? json[..MaxAuditDetails] : json;
    }

    private static void Redact(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject o:
                foreach (var key in o.Select(kv => kv.Key).ToList())
                {
                    if (SecretKey().IsMatch(key)) o[key] = "[redacted]";
                    else Redact(o[key]);
                }
                break;
            case JsonArray a:
                foreach (var item in a) Redact(item);
                break;
        }
    }

    [GeneratedRegex("password|token|secret", RegexOptions.IgnoreCase)]
    private static partial Regex SecretKey();

    [GeneratedRegex(@"^/api/data/backup/?$")]
    private static partial Regex BackupPath();

    [GeneratedRegex(@"^/api/data/export/[a-z-]+/?$")]
    private static partial Regex ExportPath();
}
