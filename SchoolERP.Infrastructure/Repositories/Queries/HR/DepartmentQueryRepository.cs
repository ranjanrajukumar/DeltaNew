using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IDepartmentQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.IJobOpeningQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.IStaffAttendanceQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class DepartmentQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IDepartmentQueryRepository
{
    public Task<List<DepartmentRow>> ListWithHeadsAsync(CancellationToken ct = default) => QueryAsync<DepartmentRow>("""
        SELECT d.id AS Id, d.name AS Name, d.head_staff_id AS HeadStaffId, s.first_name + ' ' + s.last_name AS HeadName
        FROM departments d
        LEFT JOIN staff s ON s.id = d.head_staff_id AND s.delstatus = 0
        WHERE d.delstatus = 0
        ORDER BY d.name
        """, null, ct);
}
