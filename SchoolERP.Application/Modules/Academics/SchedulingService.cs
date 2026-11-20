using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Rooms, room bookings, cover for absent teachers and the timetable report (the Node app's academics/scheduling.service.js).</summary>
public sealed class SchedulingService(IApplicationDbContext db, ICurrentUser user)
{
    private const int Days = 6; // Monday to Saturday
    private const int Periods = 8;

    private static string? NameOf(Staff? s) => s is null ? null : $"{s.FirstName} {s.LastName}";
    private static string? ClassNameOf(SchoolClass? c) => c is null ? null : $"{c.Name}-{c.Section}";

    /// <summary>Timetable day number for a date: Monday = 1 ... Sunday = 7.</summary>
    private static int Weekday(DateOnly? date) =>
        date is { } d ? (d.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)d.DayOfWeek) : throw HttpException.BadRequest("Date is not valid");

    /* ---------- Rooms ---------- */

    public async Task<List<JsonObject>> ListRoomsAsync(CancellationToken ct = default)
    {
        var today = Clock.Today();
        var rooms = await db.Set<Room>().AsNoTracking().OrderBy(r => r.Name).ToListAsync(ct);
        var slots = await db.Set<TimetableSlot>().Where(s => s.RoomId != null).Select(s => s.RoomId!.Value).ToListAsync(ct);
        var bookings = await db.Set<RoomBooking>().Where(b => b.Date >= today).Select(b => b.RoomId).ToListAsync(ct);
        return rooms.Select(r =>
        {
            var o = JsonDefaults.ToJsonObject(r);
            o["periodsPerWeek"] = slots.Count(s => s == r.Id);
            o["upcomingBookings"] = bookings.Count(b => b == r.Id);
            return o;
        }).ToList();
    }

    public async Task DeleteRoomAsync(int id, CancellationToken ct = default)
    {
        var room = await db.Set<Room>().FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw HttpException.NotFound("Room not found");
        if (await db.Set<TimetableSlot>().AnyAsync(s => s.RoomId == id, ct))
            throw HttpException.Conflict("This room is used in the timetable; clear it from those periods first");
        if (await db.Set<RoomBooking>().AnyAsync(b => b.RoomId == id, ct)) throw HttpException.Conflict("This room has bookings and cannot be deleted");
        db.Set<Room>().Remove(room);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>One room's weekly timetable use plus its bookings from today on.</summary>
    public async Task<object> RoomScheduleAsync(int id, CancellationToken ct = default)
    {
        var room = await db.Set<Room>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw HttpException.NotFound("Room not found");
        var today = Clock.Today();
        var slots = await db.Set<TimetableSlot>().AsNoTracking().Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Staff)
            .Where(s => s.RoomId == id && s.SchoolClass != null && s.Subject != null)
            .OrderBy(s => s.Day).ThenBy(s => s.Period).ToListAsync(ct);
        var bookings = await db.Set<RoomBooking>().AsNoTracking().Where(b => b.RoomId == id && b.Date >= today)
            .OrderBy(b => b.Date).ThenBy(b => b.Period).ToListAsync(ct);
        return new
        {
            room,
            slots = slots.Select(s => new
            {
                id = s.Id, day = s.Day, period = s.Period, className = ClassNameOf(s.SchoolClass), subjectName = s.Subject?.Name, teacherName = NameOf(s.Staff),
            }),
            bookings,
        };
    }

    public async Task<int> CreateBookingAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        var roomId = b.Int("roomId");
        if (roomId is null || !await db.Set<Room>().AnyAsync(r => r.Id == roomId, ct)) throw HttpException.NotFound("Room not found");
        if (b.Num("period") is not { } pn || pn != Math.Floor(pn) || pn < 1 || pn > Periods) throw HttpException.BadRequest($"Period must be 1 to {Periods}");
        var p = (int)pn;
        if (b.IsBlankTrimmed("purpose")) throw HttpException.BadRequest("Purpose is required");
        if (!b.Truthy("date") || string.CompareOrdinal(b.Str("date"), Clock.TodayText()) < 0) throw HttpException.BadRequest("Choose today or a later date");
        var date = b.Date("date");
        var day = Weekday(date);
        var lesson = await db.Set<TimetableSlot>().AsNoTracking().Include(s => s.SchoolClass)
            .FirstOrDefaultAsync(s => s.RoomId == roomId && s.Day == day && s.Period == p && s.SchoolClass != null, ct);
        if (lesson is not null) throw HttpException.Conflict($"The room is used by {ClassNameOf(lesson.SchoolClass)} in that period");
        if (await db.Set<RoomBooking>().AnyAsync(x => x.RoomId == roomId && x.Date == date && x.Period == p, ct))
            throw HttpException.Conflict("The room is already booked for that period");
        var booking = new RoomBooking { RoomId = roomId.Value, Date = date!.Value, Period = p, Purpose = b.Trimmed("purpose"), BookedByName = user.Name };
        db.Set<RoomBooking>().Add(booking);
        await db.SaveChangesAsync(ct);
        return booking.Id;
    }

    public async Task DeleteBookingAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Set<RoomBooking>().FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw HttpException.NotFound("Booking not found");
        db.Set<RoomBooking>().Remove(booking);
        await db.SaveChangesAsync(ct);
    }

    /* ---------- Substitutions ---------- */

    private async Task<HashSet<int>> OnLeaveAsync(DateOnly date, CancellationToken ct) =>
        (await db.Set<LeaveRequest>().Where(l => l.Status == "approved" && l.FromDate <= date && l.ToDate >= date).Select(l => l.StaffId).ToListAsync(ct)).ToHashSet();

    /// <summary>Active teachers with no lesson, no cover duty and no approved leave in that period of that date.</summary>
    public async Task<List<(int Id, string Name, string StaffNo)>> FreeTeachersAsync(DateOnly? date, double? period, CancellationToken ct = default)
    {
        var day = Weekday(date);
        if (period is not { } pn || pn != Math.Floor(pn) || pn < 1) throw HttpException.BadRequest("Period is required");
        var p = (int)pn;
        var staff = await db.Set<Staff>().AsNoTracking().Where(s => s.Status == "active").OrderBy(s => s.FirstName).ToListAsync(ct);
        var taken = (await db.Set<TimetableSlot>().Where(s => s.Day == day && s.Period == p && s.StaffId != null).Select(s => s.StaffId!.Value).ToListAsync(ct)).ToHashSet();
        taken.UnionWith(await db.Set<Substitution>().Where(s => s.Date == date && s.TimetableSlot != null && s.TimetableSlot.Period == p).Select(s => s.SubstituteId).ToListAsync(ct));
        taken.UnionWith(await OnLeaveAsync(date!.Value, ct));
        return staff.Where(s => !taken.Contains(s.Id)).Select(s => (s.Id, NameOf(s)!, s.StaffNo)).ToList();
    }

    public async Task<List<object>> FreeTeachersJsonAsync(DateOnly? date, double? period, CancellationToken ct = default) =>
        (await FreeTeachersAsync(date, period, ct)).Select(t => (object)new { id = t.Id, name = t.Name, staffNo = t.StaffNo }).ToList();

    /// <summary>Periods of teachers on approved leave on the date, with the cover already arranged (if any).</summary>
    public async Task<List<object>> NeededCoverAsync(DateOnly? date, CancellationToken ct = default)
    {
        var day = Weekday(date);
        var away = await OnLeaveAsync(date!.Value, ct);
        var slots = await db.Set<TimetableSlot>().AsNoTracking().Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Staff)
            .Where(s => s.Day == day && s.StaffId != null && away.Contains(s.StaffId.Value) && s.SchoolClass != null && s.Subject != null)
            .OrderBy(s => s.Period).ToListAsync(ct);
        var subs = await db.Set<Substitution>().AsNoTracking().Include(s => s.Substitute).Where(s => s.Date == date && s.Substitute != null).ToListAsync(ct);
        var bySlot = subs.GroupBy(s => s.SlotId).ToDictionary(g => g.Key, g => g.Last());
        return slots.Select(s =>
        {
            var sub = bySlot.GetValueOrDefault(s.Id);
            return (object)new
            {
                slotId = s.Id,
                period = s.Period,
                className = ClassNameOf(s.SchoolClass),
                subjectName = s.Subject?.Name,
                teacherName = NameOf(s.Staff),
                substitution = sub is null ? null : new { id = sub.Id, substituteId = sub.SubstituteId, substituteName = NameOf(sub.Substitute), reason = sub.Reason },
            };
        }).ToList();
    }

    public async Task<int> AssignSubstituteAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        var slotId = b.Int("slotId");
        var slot = slotId is null ? null : await db.Set<TimetableSlot>().AsNoTracking().FirstOrDefaultAsync(s => s.Id == slotId, ct);
        if (slot is null) throw HttpException.NotFound("Timetable period not found");
        if (!b.Truthy("date") || string.CompareOrdinal(b.Str("date"), Clock.TodayText()) < 0) throw HttpException.BadRequest("Choose today or a later date");
        var date = b.Date("date");
        if (Weekday(date) != slot.Day) throw HttpException.BadRequest("That period is not taught on that day of the week");
        var substituteId = b.Num("substituteId");
        if (substituteId == slot.StaffId) throw HttpException.BadRequest("The usual teacher cannot cover their own period");
        if (await db.Set<Substitution>().AnyAsync(s => s.SlotId == slot.Id && s.Date == date, ct))
            throw HttpException.Conflict("This period already has cover; remove it first");
        var free = await FreeTeachersAsync(date, slot.Period, ct);
        if (!free.Any(t => t.Id == substituteId))
            throw HttpException.Conflict("That teacher is not free in this period (lesson, other cover, or on leave)");
        var row = new Substitution
        {
            SlotId = slot.Id, Date = date!.Value, SubstituteId = (int)substituteId!.Value,
            Reason = b.Truthy("reason") ? b.Str("reason") : null, CreatedByName = user.Name,
        };
        db.Set<Substitution>().Add(row);
        await db.SaveChangesAsync(ct);
        return row.Id;
    }

    public async Task RemoveSubstitutionAsync(int id, CancellationToken ct = default)
    {
        var sub = await db.Set<Substitution>().FirstOrDefaultAsync(s => s.Id == id, ct) ?? throw HttpException.NotFound("Substitution not found");
        if (sub.Date < Clock.Today()) throw HttpException.Conflict("Past cover is a record and cannot be removed");
        db.Set<Substitution>().Remove(sub);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<object>> ListSubstitutionsAsync(DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var start = from ?? Clock.Today().AddDays(-30);
        var today = Clock.Today();
        var rows = await db.Set<Substitution>().AsNoTracking()
            .Include(s => s.Substitute)
            .Include(s => s.TimetableSlot).ThenInclude(t => t!.SchoolClass)
            .Include(s => s.TimetableSlot).ThenInclude(t => t!.Subject)
            .Include(s => s.TimetableSlot).ThenInclude(t => t!.Staff)
            .Where(s => s.Date >= start && (to == null || s.Date <= to) && s.Substitute != null
                && s.TimetableSlot != null && s.TimetableSlot.SchoolClass != null && s.TimetableSlot.Subject != null)
            .OrderByDescending(s => s.Date).ThenByDescending(s => s.Id)
            .ToListAsync(ct);
        return rows.Select(s => (object)new
        {
            id = s.Id,
            date = s.Date,
            period = s.TimetableSlot!.Period,
            className = ClassNameOf(s.TimetableSlot.SchoolClass),
            subjectName = s.TimetableSlot.Subject?.Name,
            teacherName = NameOf(s.TimetableSlot.Staff),
            substituteName = NameOf(s.Substitute),
            reason = s.Reason,
            removable = s.Date >= today,
        }).ToList();
    }

    /* ---------- Report ---------- */

    public async Task<object> ReportAsync(CancellationToken ct = default)
    {
        var since = Clock.Today().AddDays(-30);
        var slots = await db.Set<TimetableSlot>().AsNoTracking().Include(s => s.SchoolClass).Include(s => s.Subject).Include(s => s.Staff)
            .Where(s => s.SchoolClass != null && s.Subject != null).ToListAsync(ct);
        var staff = await db.Set<Staff>().AsNoTracking().Where(s => s.Status == "active").ToListAsync(ct);
        var rooms = await db.Set<Room>().AsNoTracking().OrderBy(r => r.Name).ToListAsync(ct);
        var classes = await db.Set<SchoolClass>().AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Section).ToListAsync(ct);
        var subs = await db.Set<Substitution>().AsNoTracking().Where(s => s.Date >= since).ToListAsync(ct);
        const int capacity = Days * Periods;
        var covered = subs.GroupBy(s => s.SubstituteId).ToDictionary(g => g.Key, g => g.Count());
        return new
        {
            periodsPerWeek = capacity,
            teacherLoad = staff
                .Select(s => new { id = s.Id, name = NameOf(s), periods = slots.Count(x => x.StaffId == s.Id), coverLast30Days = covered.GetValueOrDefault(s.Id) })
                .OrderByDescending(t => t.periods).ToList(),
            classes = classes.Select(c => new { id = c.Id, name = ClassNameOf(c), periods = slots.Count(x => x.ClassId == c.Id) }),
            unassigned = slots.Where(s => s.StaffId is null).Select(s => new
            {
                id = s.Id, className = ClassNameOf(s.SchoolClass), subjectName = s.Subject?.Name, day = s.Day, period = s.Period,
            }),
            rooms = rooms.Select(r =>
            {
                var used = slots.Count(x => x.RoomId == r.Id);
                return new { id = r.Id, name = r.Name, kind = r.Kind, periods = used, utilisationPercent = Math.Floor(used * 1000.0 / capacity + 0.5) / 10 };
            }),
            substitutionsLast30Days = subs.Count,
        };
    }
}
