using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class RoomRepository(ApplicationDbContext db) : Repository<Room>(db), IRoomRepository
{
    private DbSet<RoomBooking> Bookings => Db.Set<RoomBooking>();

    public Task<List<Room>> ListByNameAsync(CancellationToken ct = default) => Read.OrderBy(r => r.Name).ToListAsync(ct);

    public Task<List<int>> BookedRoomIdsFromAsync(DateOnly from, CancellationToken ct = default) =>
        Bookings.Where(b => b.Date >= from).Select(b => b.RoomId).ToListAsync(ct);

    public Task<bool> HasBookingsAsync(int roomId, CancellationToken ct = default) => Bookings.AnyAsync(b => b.RoomId == roomId, ct);

    public Task<List<RoomBooking>> BookingsFromAsync(int roomId, DateOnly from, CancellationToken ct = default) =>
        Bookings.AsNoTracking().Where(b => b.RoomId == roomId && b.Date >= from).OrderBy(b => b.Date).ThenBy(b => b.Period).ToListAsync(ct);

    public Task<bool> IsBookedAsync(int roomId, DateOnly? date, int period, CancellationToken ct = default) =>
        Bookings.AnyAsync(x => x.RoomId == roomId && x.Date == date && x.Period == period, ct);

    public Task<RoomBooking?> FindBookingAsync(int id, CancellationToken ct = default) => Bookings.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void AddBooking(RoomBooking booking) => Bookings.Add(booking);

    public void RemoveBooking(RoomBooking booking) => Bookings.Remove(booking);
}
