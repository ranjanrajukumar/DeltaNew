using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Authentication;

/// <summary>
/// Role rights (the Node app's platform/rights.service.js): actions an admin switched off for a role in a module
/// (view, create, edit, delete, approve, export). None are off by default, and admin is never limited.
/// </summary>
public sealed partial class RightsService(IApplicationDbContext db)
{
    public static readonly string[] Actions = ["view", "create", "edit", "delete", "approve", "export"];

    /// <summary>What a parent or student can do in the portal: each area is a set of requests under /api/portal/children/:id.</summary>
    private static readonly (string Key, Regex Test)[] PortalAreas =
    [
        ("portal-leave", new Regex(@"/leaves(/|$)")),
        ("portal-documents", new Regex(@"/documents(/|$)")),
        ("portal-feedback", new Regex(@"/feedback(/|$)")),
        ("portal-lms", new Regex(@"/lms(/|$)")),
        ("portal-activities", new Regex(@"/activities(/|$)")),
    ];

    // "role|module" -> row, kept until an admin saves a change
    private static Dictionary<string, RoleRight>? _cache;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static void ClearCache() => _cache = null;

    private async Task<Dictionary<string, RoleRight>> RightsAsync(CancellationToken ct)
    {
        if (_cache is { } cached) return cached;
        await Lock.WaitAsync(ct);
        try
        {
            return _cache ??= (await db.Set<RoleRight>().AsNoTracking().ToListAsync(ct))
                .GroupBy(r => $"{r.Role}|{r.Module}")
                .ToDictionary(g => g.Key, g => g.First());
        }
        finally
        {
            Lock.Release();
        }
    }

    private static string? PortalAreaOf(string urlPath)
    {
        var path = urlPath.Split('?')[0];
        return path.StartsWith("/api/portal/children/", StringComparison.Ordinal)
            ? PortalAreas.FirstOrDefault(a => a.Test.IsMatch(path)).Key
            : null;
    }

    /// <summary>What a request does, from its method and address.</summary>
    private static string ActionOf(string method, string urlPath)
    {
        var path = urlPath.Split('?')[0].ToLowerInvariant();
        var last = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";
        if (last is "approve" or "reject" or "accept" or "decide" && method != "GET") return "approve";
        if (last == "export" || ExportQuery().IsMatch(urlPath.ToLowerInvariant())) return "export";
        return method switch
        {
            "DELETE" => "delete",
            "PUT" or "PATCH" => "edit",
            "POST" => "create",
            _ => "view",
        };
    }

    private static bool Allowed(RoleRight row, string action) => action switch
    {
        "view" => row.CanView,
        "create" => row.CanCreate,
        "edit" => row.CanEdit,
        "delete" => row.CanDelete,
        "approve" => row.CanApprove,
        "export" => row.CanExport,
        _ => true,
    };

    /// <summary>Whether this role may export (print, PDF, Excel, CSV) from this module.</summary>
    public async Task<bool> MayExportAsync(string role, string moduleKey, CancellationToken ct = default)
    {
        if (role == "admin") return true;
        return !(await RightsAsync(ct)).TryGetValue($"{role}|{moduleKey}", out var row) || row.CanExport;
    }

    /// <summary>False only when an admin has switched this action off for this role in this module.</summary>
    public async Task<bool> AllowsAsync(string role, string method, string urlPath, CancellationToken ct = default)
    {
        if (role == "admin") return true;
        // building a saved report as a file is checked as an export by the report routes themselves
        if (method == "POST" && ReportBuild().IsMatch(urlPath.Split('?')[0])) return true;
        var module = AccessRules.ModuleOfPath(urlPath) ?? PortalAreaOf(urlPath);
        if (module is null || !(await RightsAsync(ct)).TryGetValue($"{role}|{module}", out var row)) return true;
        return Allowed(row, ActionOf(method, urlPath));
    }

    [GeneratedRegex("[?&](format=csv|export=)")]
    private static partial Regex ExportQuery();

    [GeneratedRegex(@"^/api/report-designer/(runs|print|templates/\d+/(csv|run))/?$")]
    private static partial Regex ReportBuild();
}
