using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IAttendanceQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ILeaveBalanceQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class AttendanceQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IAttendanceQueryRepository
{
    public Task<List<RosterRow>> RosterAsync(DateOnly date, int? classId, CancellationToken ct = default) => QueryAsync<RosterRow>("""
        SELECT s.id AS StudentId, s.admission_no AS AdmissionNo, s.first_name AS FirstName, s.last_name AS LastName,
               a.status AS Status, a.arrival_time AS ArrivalTime, a.note AS Note, a.saved_at AS SavedAt,
               s.class_id AS ClassId, c.name AS ClassName, c.section AS Section
        FROM students s
        LEFT JOIN classes c ON c.id = s.class_id AND c.delstatus = 0
        OUTER APPLY (SELECT TOP 1 status, arrival_time, note, saved_at FROM attendance
                     WHERE student_id = s.id AND date = @date AND delstatus = 0) a
        WHERE s.delstatus = 0 AND s.status = 'active' AND (@classId IS NULL OR s.class_id = @classId)
        ORDER BY s.first_name
        """, new { date, classId }, ct);

    public Task<List<CorrectionRow>> CorrectionsAsync(int? studentId, DateOnly? from, DateOnly? to, CancellationToken ct = default) => QueryAsync<CorrectionRow>("""
        SELECT TOP 300 x.id AS Id, x.date AS Date, x.student_id AS StudentId, s.first_name AS FirstName, s.last_name AS LastName,
               s.admission_no AS AdmissionNo, x.old_status AS OldStatus, x.new_status AS NewStatus, x.reason AS Reason,
               x.corrected_by_name AS CorrectedByName, x.created_at AS CreatedAt
        FROM attendance_corrections x
        JOIN students s ON s.id = x.student_id AND s.delstatus = 0
        WHERE x.delstatus = 0 AND (@studentId IS NULL OR x.student_id = @studentId)
          AND (@from IS NULL OR x.date >= @from) AND (@to IS NULL OR x.date <= @to)
        ORDER BY x.id DESC
        """, new { studentId, from, to }, ct);

    public Task<List<PeriodSummaryRow>> PeriodSummaryAsync(int classId, DateOnly from, DateOnly to, CancellationToken ct = default) => QueryAsync<PeriodSummaryRow>("""
        SELECT s.id AS Id, s.admission_no AS AdmissionNo, s.first_name AS FirstName, s.last_name AS LastName,
               COUNT(p.id) AS Periods,
               SUM(CASE WHEN p.status = 'absent' THEN 1 ELSE 0 END) AS Absent,
               SUM(CASE WHEN p.status = 'late' THEN 1 ELSE 0 END) AS Late
        FROM students s
        LEFT JOIN period_attendance p ON p.student_id = s.id AND p.delstatus = 0 AND p.date >= @from AND p.date <= @to
        WHERE s.delstatus = 0 AND s.class_id = @classId AND s.status = 'active'
        GROUP BY s.id, s.admission_no, s.first_name, s.last_name
        ORDER BY s.id
        """, new { classId, from, to }, ct);
}
