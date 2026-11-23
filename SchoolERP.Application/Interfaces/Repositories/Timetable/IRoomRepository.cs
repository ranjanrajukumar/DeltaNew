using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Rooms and room bookings (EF Core).</summary>
public interface IRoomRepository : IRepository<Room>
{
    /// <summary>Every room by name (read-only).</summary>
    Task<List<Room>> ListByNameAsync(CancellationToken ct = default);

    /// <summary>Room id of every booking on or after <paramref name="from"/>.</summary>
    Task<List<int>> BookedRoomIdsFromAsync(DateOnly from, CancellationToken ct = default);

    Task<bool> HasBookingsAsync(int roomId, CancellationToken ct = default);

    /// <summary>A room's bookings on or after <paramref name="from"/>, by date then period (read-only).</summary>
    Task<List<RoomBooking>> BookingsFromAsync(int roomId, DateOnly from, CancellationToken ct = default);

    Task<bool> IsBookedAsync(int roomId, DateOnly? date, int period, CancellationToken ct = default);

    /// <summary>Tracked booking, or null.</summary>
    Task<RoomBooking?> FindBookingAsync(int id, CancellationToken ct = default);

    void AddBooking(RoomBooking booking);

    void RemoveBooking(RoomBooking booking);
}
