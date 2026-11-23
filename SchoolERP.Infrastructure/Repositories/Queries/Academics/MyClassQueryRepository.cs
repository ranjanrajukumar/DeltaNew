using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IHomeworkQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ISyllabusQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class MyClassQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IMyClassQueryRepository
{
    public Task<(List<IMyClassQueryRepository.RollRow> Roll, List<IMyClassQueryRepository.LeaveRow> Leaves)> TodayAsync(int classId, DateOnly date, CancellationToken ct = default) =>
        QueryMultipleAsync("""
        SELECT s.id AS Id, s.admission_no AS AdmissionNo, s.first_name AS FirstName, s.last_name AS LastName,
               s.guardian_name AS GuardianName, s.guardian_phone AS GuardianPhone, a.status AS Today
        FROM students s
        OUTER APPLY (SELECT TOP 1 status FROM attendance WHERE student_id = s.id AND date = @date AND delstatus = 0) a
        WHERE s.class_id = @classId AND s.status = 'active' AND s.delstatus = 0
        ORDER BY s.first_name, s.last_name;
        SELECT l.student_id AS StudentId, l.status AS Status, l.from_date AS FromDate, l.to_date AS ToDate
        FROM student_leaves l
        JOIN students s ON s.id = l.student_id AND s.delstatus = 0
        WHERE l.delstatus = 0 AND s.class_id = @classId AND s.status = 'active'
          AND l.status IN ('pending', 'approved') AND l.to_date >= @date
        ORDER BY l.id;
        """, new { classId, date }, async grid => (
            (await grid.ReadAsync<IMyClassQueryRepository.RollRow>()).AsList(),
            (await grid.ReadAsync<IMyClassQueryRepository.LeaveRow>()).AsList()), ct);
}
