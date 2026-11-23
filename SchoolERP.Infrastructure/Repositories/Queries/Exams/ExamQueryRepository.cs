using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IExamQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class ExamQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IExamQueryRepository
{
    public Task<(List<GroupRow> Groups, List<GroupExamRow> Exams)> GroupsAsync(CancellationToken ct = default) => QueryMultipleAsync("""
        SELECT g.id AS Id, g.class_id AS ClassId, g.name AS Name, c.name + '-' + c.section AS ClassName
        FROM exam_groups g
        JOIN classes c ON c.id = g.class_id AND c.delstatus = 0
        WHERE g.delstatus = 0
        ORDER BY g.id DESC;
        SELECT e.id AS Id, e.name AS Name, e.weight AS Weight, e.exam_group_id AS GroupId
        FROM exams e
        WHERE e.delstatus = 0 AND e.exam_group_id IN (
            SELECT g.id FROM exam_groups g JOIN classes c ON c.id = g.class_id AND c.delstatus = 0 WHERE g.delstatus = 0)
        ORDER BY e.id;
        """, null, async grid => (
            (await grid.ReadAsync<GroupRow>()).AsList(),
            (await grid.ReadAsync<GroupExamRow>()).AsList()), ct);

    public async Task<AttendanceSummaryRow> AttendanceSummaryAsync(int studentId, CancellationToken ct = default) =>
        (await QuerySingleOrDefaultAsync<AttendanceSummaryRow>("""
        SELECT COUNT(*) AS Days, COUNT(CASE WHEN a.status <> 'absent' THEN 1 END) AS Attended
        FROM attendance a
        WHERE a.delstatus = 0 AND a.student_id = @studentId
        """, new { studentId }, ct))!;

    public Task<List<StudentAttendanceRow>> AttendanceByStudentAsync(int classId, CancellationToken ct = default) => QueryAsync<StudentAttendanceRow>("""
        SELECT a.student_id AS StudentId, COUNT(*) AS Total, COUNT(CASE WHEN a.status <> 'absent' THEN 1 END) AS Present
        FROM attendance a
        JOIN students s ON s.id = a.student_id AND s.delstatus = 0
        WHERE a.delstatus = 0 AND s.class_id = @classId AND s.status = 'active'
        GROUP BY a.student_id
        """, new { classId }, ct);
}
