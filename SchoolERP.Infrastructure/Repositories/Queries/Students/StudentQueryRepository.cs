using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IStudentQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class StudentQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IStudentQueryRepository
{
    public async Task<RegistrationSummaryRow> RegistrationSummaryAsync(CancellationToken ct = default) =>
        (await QuerySingleOrDefaultAsync<RegistrationSummaryRow>("""
        SELECT (SELECT COUNT(*) FROM students WHERE delstatus = 0) AS Total,
               (SELECT COUNT(*) FROM students WHERE delstatus = 0 AND registration_status = 'complete') AS Complete
        """, null, ct))!;

    public async Task<SummaryRow> SummaryAsync(DateTime monthStart, CancellationToken ct = default) =>
        (await QuerySingleOrDefaultAsync<SummaryRow>("""
        SELECT (SELECT COUNT(*) FROM students WHERE delstatus = 0) AS Total,
               (SELECT COUNT(*) FROM students WHERE delstatus = 0 AND status = 'active') AS Active,
               (SELECT COUNT(*) FROM students WHERE delstatus = 0 AND created_at >= @monthStart) AS AddedThisMonth,
               (SELECT COUNT(*) FROM admissions WHERE deleted_at IS NULL AND status = 'approved') AS PendingEnrollment,
               (SELECT COUNT(*) FROM classes WHERE delstatus = 0) AS Classes
        """, new { monthStart }, ct))!;
}
