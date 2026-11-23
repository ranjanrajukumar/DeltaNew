using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Role rights: actions an admin switched off for a role in a module (the Node app's platform/rights.service.js).</summary>
public interface IRightsService
{
    /// <summary>Whether this role may export (print, PDF, Excel, CSV) from this module.</summary>
    Task<bool> MayExportAsync(string role, string moduleKey, CancellationToken ct = default);

    /// <summary>False only when an admin has switched this action off for this role in this module.</summary>
    Task<bool> AllowsAsync(string role, string method, string urlPath, CancellationToken ct = default);
}
