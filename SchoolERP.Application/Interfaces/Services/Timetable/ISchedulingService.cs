using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Rooms, room bookings, cover for absent teachers and the timetable report.</summary>
public interface ISchedulingService
{
    Task<List<JsonObject>> ListRoomsAsync(CancellationToken ct = default);

    Task DeleteRoomAsync(int id, CancellationToken ct = default);

    Task<object> RoomScheduleAsync(int id, CancellationToken ct = default);

    Task<int> CreateBookingAsync(JsonObject? json, CancellationToken ct = default);

    Task DeleteBookingAsync(int id, CancellationToken ct = default);

    /// <summary>Active teachers with no lesson, no cover duty and no approved leave in that period of that date.</summary>
    Task<List<(int Id, string Name, string StaffNo)>> FreeTeachersAsync(DateOnly? date, double? period, CancellationToken ct = default);

    Task<List<object>> FreeTeachersJsonAsync(DateOnly? date, double? period, CancellationToken ct = default);

    Task<List<object>> NeededCoverAsync(DateOnly? date, CancellationToken ct = default);

    Task<int> AssignSubstituteAsync(JsonObject? json, CancellationToken ct = default);

    Task RemoveSubstitutionAsync(int id, CancellationToken ct = default);

    Task<List<object>> ListSubstitutionsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default);

    Task<object> ReportAsync(CancellationToken ct = default);
}
