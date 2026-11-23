using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IDepartmentQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.IJobOpeningQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.IStaffAttendanceQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class StaffAttendanceQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IStaffAttendanceQueryRepository
{
    public Task<List<StatusCountRow>> StatusCountsAsync(DateOnly from, DateOnly to, CancellationToken ct = default) => QueryAsync<StatusCountRow>("""
        SELECT a.staff_id AS StaffId, a.status AS Status, COUNT(*) AS Count
        FROM staff_attendance a
        WHERE a.delstatus = 0 AND a.date >= @from AND a.date <= @to
        GROUP BY a.staff_id, a.status
        """, new { from, to }, ct);
}
