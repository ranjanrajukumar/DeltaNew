using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.ISchedulingQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ITimetableQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class SchedulingQueryRepository(IDapperContext dapper) : QueryRepository(dapper), ISchedulingQueryRepository
{
    public Task<List<SubstitutionRow>> SubstitutionsAsync(DateOnly from, DateOnly? to, CancellationToken ct = default) => QueryAsync<SubstitutionRow>("""
        SELECT x.id AS Id, x.date AS Date, CAST(s.period AS int) AS Period, c.name + '-' + c.section AS ClassName, sub.name AS SubjectName,
               t.first_name + ' ' + t.last_name AS TeacherName, sb.first_name + ' ' + sb.last_name AS SubstituteName, x.reason AS Reason
        FROM substitutions x
        JOIN staff sb ON sb.id = x.substitute_id AND sb.delstatus = 0
        JOIN timetable_slots s ON s.id = x.slot_id AND s.delstatus = 0
        JOIN classes c ON c.id = s.class_id AND c.delstatus = 0
        JOIN subjects sub ON sub.id = s.subject_id AND sub.delstatus = 0
        LEFT JOIN staff t ON t.id = s.staff_id AND t.delstatus = 0
        WHERE x.delstatus = 0 AND x.date >= @from AND (@to IS NULL OR x.date <= @to)
        ORDER BY x.date DESC, x.id DESC
        """, new { from, to }, ct);
}
