using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IAttendanceQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ILeaveBalanceQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class LeaveBalanceQueryRepository(IDapperContext dapper) : QueryRepository(dapper), ILeaveBalanceQueryRepository
{
    public Task<List<UsedRow>> UsedAsync(string kind, DateOnly start, DateOnly end, CancellationToken ct = default) => QueryAsync<UsedRow>(kind == "staff"
        ? """
          SELECT staff_id AS PersonId, leave_type_id AS TypeId, SUM(days) AS Days
          FROM leave_requests
          WHERE delstatus = 0 AND status = 'approved' AND from_date >= @start AND to_date <= @end AND leave_type_id IS NOT NULL
          GROUP BY staff_id, leave_type_id
          """
        : """
          SELECT student_id AS PersonId, leave_type_id AS TypeId, SUM(days) AS Days
          FROM student_leaves
          WHERE delstatus = 0 AND status = 'approved' AND from_date >= @start AND to_date <= @end AND leave_type_id IS NOT NULL
          GROUP BY student_id, leave_type_id
          """, new { start, end }, ct);

    public Task<List<PersonRow>> ActiveStaffAsync(CancellationToken ct = default) => QueryAsync<PersonRow>("""
        SELECT id AS Id, staff_no AS No, first_name AS FirstName, last_name AS LastName, designation AS [Group]
        FROM staff
        WHERE delstatus = 0 AND status = 'active'
        ORDER BY first_name
        """, null, ct);

    public Task<List<PersonRow>> ActiveStudentsAsync(CancellationToken ct = default) => QueryAsync<PersonRow>("""
        SELECT s.id AS Id, s.admission_no AS No, s.first_name AS FirstName, s.last_name AS LastName,
               c.name + '-' + c.section AS [Group]
        FROM students s
        LEFT JOIN classes c ON c.id = s.class_id AND c.delstatus = 0
        WHERE s.delstatus = 0 AND s.status = 'active'
        ORDER BY s.first_name
        """, null, ct);
}
