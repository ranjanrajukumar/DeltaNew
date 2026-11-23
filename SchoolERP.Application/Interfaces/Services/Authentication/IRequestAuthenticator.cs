using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>The Node app's requireAuth: loads the user named by a validated token and checks their access.</summary>
public interface IRequestAuthenticator
{
    /// <summary>Null when the request may go on; otherwise the status and JSON body to answer with.</summary>
    Task<(int Status, object Body)?> AuthenticateAsync(int? userId, string method, string originalUrl, CancellationToken ct = default);
}
