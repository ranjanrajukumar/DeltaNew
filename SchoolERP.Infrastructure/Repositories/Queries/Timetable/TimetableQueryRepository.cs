using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.ISchedulingQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ITimetableQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class TimetableQueryRepository(IDapperContext dapper) : QueryRepository(dapper), ITimetableQueryRepository
{
    public Task<List<SlotRow>> WeekAsync(int? classId, int? staffId, CancellationToken ct = default) => QueryAsync<SlotRow>("""
        SELECT s.id AS Id, s.class_id AS ClassId, CAST(s.day AS int) AS Day, CAST(s.period AS int) AS Period, s.subject_id AS SubjectId,
               s.staff_id AS StaffId, s.room_id AS RoomId, r.name AS RoomName, c.name + '-' + c.section AS ClassName, sub.name AS SubjectName,
               t.first_name + ' ' + t.last_name AS TeacherName
        FROM timetable_slots s
        JOIN classes c ON c.id = s.class_id AND c.delstatus = 0
        JOIN subjects sub ON sub.id = s.subject_id AND sub.delstatus = 0
        LEFT JOIN staff t ON t.id = s.staff_id AND t.delstatus = 0
        LEFT JOIN rooms r ON r.id = s.room_id AND r.delstatus = 0
        WHERE s.delstatus = 0 AND (@classId IS NULL OR s.class_id = @classId) AND (@staffId IS NULL OR s.staff_id = @staffId)
        ORDER BY s.day, s.period
        """, new { classId, staffId }, ct);
}
