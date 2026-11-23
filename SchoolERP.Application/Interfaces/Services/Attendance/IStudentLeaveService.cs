using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Attendance;
using SchoolERP.Application.Modules.Leave;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Student leave: applying, deciding and cancelling (the Node app's attendance/studentLeave.service.js).</summary>
public interface IStudentLeaveService
{
    /// <summary>The student's own login and their parents' logins.</summary>
    Task<List<int?>> FamilyUserIdsAsync(int studentId, CancellationToken ct = default);

    Task<List<JsonObject>> ListAsync(string? status, int? studentId, int? classId, StudentLeaveService.Decider? viewer, CancellationToken ct = default);

    Task<int> ApplyAsync(Body body, int? requestedBy, string requestedByRole = "staff", CancellationToken ct = default);

    Task DecideAsync(int id, string decision, string? remarks, StudentLeaveService.Decider decider, CancellationToken ct = default);

    Task CancelAsync(int id, int? studentId = null, CancellationToken ct = default);
}
