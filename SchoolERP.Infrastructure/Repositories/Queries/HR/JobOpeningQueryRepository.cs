using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IDepartmentQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.IJobOpeningQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.IStaffAttendanceQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class JobOpeningQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IJobOpeningQueryRepository
{
    public Task<List<OpeningRow>> ListAsync(CancellationToken ct = default) => QueryAsync<OpeningRow>("""
        SELECT o.id AS Id, o.title AS Title, o.department_id AS DepartmentId, d.name AS DepartmentName, o.vacancies AS Vacancies,
               o.description AS Description, o.closing_date AS ClosingDate, o.status AS Status,
               (SELECT COUNT(*) FROM job_applicants a WHERE a.opening_id = o.id AND a.delstatus = 0) AS Applicants,
               (SELECT COUNT(*) FROM job_applicants a WHERE a.opening_id = o.id AND a.delstatus = 0 AND a.status = N'hired') AS Hired
        FROM job_openings o
        LEFT JOIN departments d ON d.id = o.department_id AND d.delstatus = 0
        WHERE o.delstatus = 0
        ORDER BY o.id DESC
        """, null, ct);
}
