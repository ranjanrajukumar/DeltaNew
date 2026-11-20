using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Domain.Entities;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Application.Modules.Messaging;

/// <summary>Email and SMS to families and staff (the Node app's messaging/messaging.service.js).</summary>
public sealed partial class MessagingService(IApplicationDbContext db, IMessageDelivery delivery, SettingsReader settings)
{
    public static readonly string[] Channels = ["email", "sms"];
    private const int SmsMax = 1000;

    /// <summary>The acting user for a message or log row: a signed-in user, or the system (id null, "System").</summary>
    public sealed record Sender(int? Id, string? Name)
    {
        public static readonly Sender System = new(null, "System");
    }

    public static string ClassName(SchoolClass? c) => c is null ? "" : $"{c.Name}-{c.Section}";

    /// <summary>Fills {{placeholders}}; an unknown or missing value becomes empty rather than showing the braces.</summary>
    public static string Render(string? text, IReadOnlyDictionary<string, string?> vars) =>
        Placeholder().Replace(text ?? "", m => vars.GetValueOrDefault(m.Groups[1].Value) ?? "");

    public static bool IsEmail(string? value) => EmailShape().IsMatch(value ?? "");

    public static JsonObject Shape(Message m) => new()
    {
        ["id"] = m.Id,
        ["channel"] = m.Channel,
        ["kind"] = m.Kind,
        ["recipient"] = m.Recipient,
        ["recipientName"] = m.RecipientName,
        ["studentId"] = m.StudentId,
        ["subject"] = m.Subject,
        ["body"] = m.Body,
        ["status"] = m.Status,
        ["provider"] = m.Provider,
        ["error"] = m.Error,
        ["sentByName"] = m.SentByName,
        ["createdAt"] = JsonDefaults.Node(m.CreatedAt),
    };

    /// <summary>
    /// Validates, delivers and records one message. A delivery failure is recorded (status "failed"), not thrown; only bad
    /// input throws.
    /// </summary>
    public async Task<Message> SendMessageAsync(string? channel, string? to, string? toName, string? subject, string? body,
        string kind = "direct", int? studentId = null, string? sentByName = null, CancellationToken ct = default)
    {
        if (channel is null || !Channels.Contains(channel)) throw HttpException.BadRequest("Channel must be email or sms");
        var text = (body ?? "").Trim();
        if (text == "") throw HttpException.BadRequest("Message text is required");
        if (channel == "sms" && text.Length > SmsMax) throw HttpException.BadRequest($"SMS text is limited to {SmsMax} characters");
        if (channel == "email" && string.IsNullOrWhiteSpace(subject)) throw HttpException.BadRequest("Email subject is required");

        var address = channel == "sms" ? delivery.NormalizePhone(to) : (to ?? "").Trim();
        if (channel == "sms" && address is null)
            throw HttpException.BadRequest("Phone number is invalid. Include the country code (e.g. +91...) or set DEFAULT_COUNTRY_CODE in the server settings");
        if (channel == "email" && !IsEmail(address)) throw HttpException.BadRequest("Email address is invalid");

        var result = await delivery.DeliverAsync(channel, address!, subject?.Trim(), text, ct);
        var row = new Message
        {
            Channel = channel,
            Kind = kind,
            Recipient = address!,
            RecipientName = string.IsNullOrEmpty(toName) ? null : toName,
            StudentId = studentId,
            Subject = channel == "email" ? subject!.Trim() : null,
            Body = text,
            Status = result.Status,
            Provider = result.Provider,
            Error = result.Error,
            SentByName = sentByName,
        };
        db.Set<Message>().Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    /// <summary>
    /// Texts a student's guardian about an absence. Only a real delivery counts as "guardian informed" in the absence-alert log;
    /// in log-only mode nothing was delivered, so the log is left alone.
    /// </summary>
    public async Task<Message> SendAbsenceAlertAsync(int studentId, DateOnly? date, Sender user, string? template = null, CancellationToken ct = default)
    {
        var day = date ?? Clock.Today();
        if (!await db.Set<AttendanceMark>().AnyAsync(a => a.StudentId == studentId && a.Date == day && a.Status == "absent", ct))
            throw HttpException.BadRequest("That student is not marked absent on this date");
        if (await db.Set<AbsenceAlert>().AnyAsync(a => a.StudentId == studentId && a.Date == day && a.Method == "sms", ct))
            throw HttpException.Conflict("An SMS was already sent for this absence");

        var student = await db.Set<Student>().AsNoTracking().Include(s => s.SchoolClass).FirstAsync(s => s.Id == studentId, ct);
        if (delivery.NormalizePhone(student.GuardianPhone) is null)
            throw HttpException.BadRequest("This student has no valid guardian phone number");

        var vars = new Dictionary<string, string?>
        {
            ["school"] = await settings.GetStringAsync("school.name", ct),
            ["guardian"] = string.IsNullOrEmpty(student.GuardianName) ? "Parent" : student.GuardianName,
            ["student"] = $"{student.FirstName} {student.LastName}",
            ["class"] = ClassName(student.SchoolClass),
            ["date"] = day.ToString("yyyy-MM-dd"),
        };
        var text = Render(string.IsNullOrEmpty(template) ? await settings.GetStringAsync("notify.absenceTemplate", ct) : template, vars);
        var message = await SendMessageAsync("sms", student.GuardianPhone, student.GuardianName, null, text, "absence", studentId, user.Name, ct);
        if (message.Status == "sent")
        {
            db.Set<AbsenceAlert>().Add(new AbsenceAlert
            {
                StudentId = studentId, Date = day, Method = "sms", Note = "Automatic SMS", NotifiedBy = user.Id, NotifiedByName = user.Name,
            });
            await db.SaveChangesAsync(ct);
        }
        return message;
    }

    [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}")]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex EmailShape();
}
