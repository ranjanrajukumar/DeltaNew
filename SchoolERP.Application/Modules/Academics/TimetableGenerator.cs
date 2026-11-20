namespace SchoolERP.Application.Modules.Academics;

/// <summary>
/// Constraint-based timetable generator (the Node app's academics/timetableGenerator.js). Pure: takes plain data, returns the
/// slots it could place and what it could not.
/// Hard rules: a class, a teacher and a room each hold one period at a time; a teacher has a daily limit; practicals go in a
/// lab, as a double period. Soft rules (scored, lowest wins): a subject once a day, no same subject back to back, an even spread
/// over the week, an even load on each day. It runs several randomised attempts and keeps the one with the fewest unplaced
/// periods, then the fewest free periods inside a class's day.
/// </summary>
public static class TimetableGenerator
{
    public sealed record ClassRef(int Id, string Label);

    public sealed record Demand(int ClassId, int SubjectId, string? SubjectName, int? StaffId, int Periods, bool Practical);

    public sealed record Busy(int ClassId, int Day, int Period, int? StaffId, int? RoomId);

    public sealed record Slot(int ClassId, int Day, int Period, int SubjectId, int? StaffId, int? RoomId);

    public sealed record Unplaced(int ClassId, string? ClassName, int SubjectId, string? Subject, int Periods);

    public sealed record Result(List<Slot> Slots, List<Unplaced> Unplaced);

    /// <summary>mulberry32, exactly as the Node app seeds it, so a run can be repeated.</summary>
    private static Func<double> Prng(uint seed)
    {
        var s = seed == 0 ? 1u : seed;
        return () =>
        {
            unchecked
            {
                s += 0x6D2B79F5;
                var t = s;
                t = (t ^ (t >> 15)) * (t | 1);
                t ^= t + (t ^ (t >> 7)) * (t | 61);
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        };
    }

    public static Result Generate(
        IReadOnlyList<ClassRef> classes, IReadOnlyList<Demand> demands, IReadOnlyList<int> periods, IReadOnlyList<int> days,
        IReadOnlyList<int> labs, IReadOnlyList<Busy> busy, IReadOnlySet<(int Staff, int Day, int Period)> blocked,
        int maxTeacherPerDay = 6, int attempts = 12, uint seed = 1)
    {
        var rand = Prng(seed);
        var classLabel = classes.ToDictionary(c => c.Id, c => c.Label);
        Result? best = null;
        var bestCost = double.PositiveInfinity;

        // an unplaced period costs far more than a free period in the middle of a class's day
        double Cost(Result r)
        {
            var gaps = 0;
            foreach (var list in r.Slots.GroupBy(x => (x.ClassId, x.Day)).Select(g => g.Select(x => IndexOf(periods, x.Period)).OrderBy(i => i).ToList()))
                for (var n = 1; n < list.Count; n++) gaps += list[n] - list[n - 1] - 1;
            return r.Unplaced.Count * 1000 + gaps;
        }

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var result = RunOnce(attempt == 0 ? null : rand);
            var c = Cost(result);
            if (c < bestCost)
            {
                best = result;
                bestCost = c;
            }
            if (bestCost == 0) break;
        }
        return best!;

        Result RunOnce(Func<double>? random)
        {
            var classBusy = new HashSet<(int, int, int)>();
            var teacherBusy = new HashSet<(int, int, int)>();
            var roomBusy = new HashSet<(int, int, int)>();
            var teacherDay = new Dictionary<(int, int), int>();
            var bySubjectDay = new Dictionary<(int, int, int), int>();
            var classDay = new Dictionary<(int, int), int>();
            var slots = new List<Slot>();
            var unplaced = new List<Unplaced>();

            foreach (var b in busy)
            {
                classBusy.Add((b.ClassId, b.Day, b.Period));
                if (b.StaffId is { } st and not 0)
                {
                    teacherBusy.Add((st, b.Day, b.Period));
                    teacherDay[(st, b.Day)] = teacherDay.GetValueOrDefault((st, b.Day)) + 1;
                }
                if (b.RoomId is { } r and not 0) roomBusy.Add((r, b.Day, b.Period));
                classDay[(b.ClassId, b.Day)] = classDay.GetValueOrDefault((b.ClassId, b.Day)) + 1;
            }

            // heavily loaded teachers are placed first, as they are the hardest to fit
            var load = new Dictionary<int, int>();
            foreach (var d in demands) if (d.StaffId is { } s) load[s] = load.GetValueOrDefault(s) + d.Periods;

            // one unit = one period, or one double period for a practical
            var units = new List<(Demand D, int Size)>();
            foreach (var d in demands)
            {
                if (d.Practical && d.Periods >= 2)
                {
                    for (var i = 0; i < d.Periods / 2; i++) units.Add((d, 2));
                    if (d.Periods % 2 == 1) units.Add((d, 1));
                }
                else for (var i = 0; i < d.Periods; i++) units.Add((d, 1));
            }
            // The Node comparator drew a fresh random number on every comparison, so its shuffled attempts cannot be reproduced
            // exactly; here each unit draws one number up front, which shuffles the same way and is repeatable for a seed.
            // The first attempt has no randomness and matches Node's.
            var weighted = units.Select(u => (Unit: u, W: (u.Size == 2 ? 1000 : 0) + (u.D.StaffId is { } s ? load.GetValueOrDefault(s) : 0)
                + (random is null ? 0 : random() * 30))).ToList();
            var ordered = weighted.OrderByDescending(x => x.W).Select(x => x.Unit).ToList(); // stable, like Array.prototype.sort

            int GapPenalty(int classId, int day, int[] span)
            {
                // positions in the list of class periods: a break between two class periods is not a gap
                var held = periods.Select((n, i) => classBusy.Contains((classId, day, n)) ? i : -1).Where(i => i >= 0).ToList();
                if (held.Count == 0) return 0;
                var first = IndexOf(periods, span[0]);
                var last = IndexOf(periods, span[^1]);
                return held.Any(i => i == first - 1 || i == last + 1) ? 0 : 25;
            }

            int? Lab(int day, int p, int size) =>
                labs.Select(id => (int?)id).FirstOrDefault(id => Enumerable.Range(0, size).All(i => !roomBusy.Contains((id!.Value, day, periods[p + i]))));

            foreach (var (d, size) in ordered)
            {
                (int Day, int P, int[] Span, int? Room, double Score)? pick = null;
                foreach (var day in days)
                {
                    for (var p = 0; p + size <= periods.Count; p++)
                    {
                        var span = Enumerable.Range(0, size).Select(i => periods[p + i]).ToArray();
                        // a double period must be back to back: a break row between two class periods leaves a gap in the numbers
                        if (span[size - 1] - span[0] != size - 1) continue;
                        if (span.Any(n => classBusy.Contains((d.ClassId, day, n)))) continue;
                        if (d.StaffId is { } staff)
                        {
                            if (span.Any(n => teacherBusy.Contains((staff, day, n)))) continue;
                            if (span.Any(n => blocked.Contains((staff, day, n)))) continue;
                            if (teacherDay.GetValueOrDefault((staff, day)) + size > maxTeacherPerDay) continue;
                        }
                        int? room = null;
                        if (d.Practical && labs.Count > 0)
                        {
                            room = Lab(day, p, size);
                            if (room is null) continue;
                        }
                        var sameToday = bySubjectDay.GetValueOrDefault((d.ClassId, d.SubjectId, day));
                        bool Touches(int index) =>
                            index >= 0 && index < periods.Count
                            && slots.Any(s => s.ClassId == d.ClassId && s.SubjectId == d.SubjectId && s.Day == day && s.Period == periods[index]);
                        var adjacent = Touches(p - 1) || Touches(p + size);
                        var score = sameToday * 100
                            + (adjacent && !(d.Practical && size == 1) ? 40 : 0)
                            + classDay.GetValueOrDefault((d.ClassId, day)) * 6
                            + (d.StaffId is { } st2 ? teacherDay.GetValueOrDefault((st2, day)) * 3 : 0)
                            + p * 0.5
                            + GapPenalty(d.ClassId, day, span)
                            + (random is null ? 0 : random() * 8);
                        if (pick is null || score < pick.Value.Score) pick = (day, p, span, room, score);
                    }
                }
                if (pick is not { } chosen)
                {
                    unplaced.Add(new Unplaced(d.ClassId, classLabel.GetValueOrDefault(d.ClassId), d.SubjectId, d.SubjectName, size));
                    continue;
                }
                foreach (var n in chosen.Span)
                {
                    classBusy.Add((d.ClassId, chosen.Day, n));
                    if (d.StaffId is { } st3) teacherBusy.Add((st3, chosen.Day, n));
                    if (chosen.Room is { } rm) roomBusy.Add((rm, chosen.Day, n));
                    slots.Add(new Slot(d.ClassId, chosen.Day, n, d.SubjectId, d.StaffId, chosen.Room));
                }
                bySubjectDay[(d.ClassId, d.SubjectId, chosen.Day)] = bySubjectDay.GetValueOrDefault((d.ClassId, d.SubjectId, chosen.Day)) + size;
                classDay[(d.ClassId, chosen.Day)] = classDay.GetValueOrDefault((d.ClassId, chosen.Day)) + size;
                if (d.StaffId is { } st4) teacherDay[(st4, chosen.Day)] = teacherDay.GetValueOrDefault((st4, chosen.Day)) + size;
            }
            return new Result(slots, unplaced);
        }
    }

    private static int IndexOf(IReadOnlyList<int> list, int value)
    {
        for (var i = 0; i < list.Count; i++) if (list[i] == value) return i;
        return -1;
    }
}
