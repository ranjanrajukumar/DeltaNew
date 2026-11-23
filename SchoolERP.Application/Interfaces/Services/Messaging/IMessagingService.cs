using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Email and SMS to families and staff (the Node app's messaging/messaging.service.js).</summary>
public interface IMessagingService
{
    /// <summary>Validates, delivers and records one message; a delivery failure is recorded, not thrown.</summary>
    Task<Message> SendMessageAsync(string? channel, string? to, string? toName, string? subject, string? body,
        string kind = "direct", int? studentId = null, string? sentByName = null, CancellationToken ct = default);

    /// <summary>Texts a student's guardian about an absence.</summary>
    Task<Message> SendAbsenceAlertAsync(int studentId, DateOnly? date, MessagingService.Sender user, string? template = null, CancellationToken ct = default);
}
