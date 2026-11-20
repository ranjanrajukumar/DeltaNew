using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IHomeworkQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ISyllabusQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class HomeworkQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IHomeworkQueryRepository
{
    public Task<List<HomeworkRow>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default) => QueryAsync<HomeworkRow>("""
        SELECT TOP 300 h.id AS Id, h.class_id AS ClassId, h.subject_id AS SubjectId, h.title AS Title, h.description AS Description,
               h.assigned_on AS AssignedOn, h.due_date AS DueDate, h.max_marks AS MaxMarks, h.created_by_name AS CreatedByName,
               c.name + '-' + c.section AS ClassName, s.name AS SubjectName,
               (SELECT COUNT(*) FROM homework_submissions x WHERE x.homework_id = h.id AND x.delstatus = 0 AND x.status IN ('submitted', 'late')) AS Submitted,
               (SELECT COUNT(*) FROM homework_submissions x WHERE x.homework_id = h.id AND x.delstatus = 0) AS Total
        FROM homework h
        JOIN classes c ON c.id = h.class_id AND c.delstatus = 0
        JOIN subjects s ON s.id = h.subject_id AND s.delstatus = 0
        WHERE h.delstatus = 0 AND (@classId IS NULL OR h.class_id = @classId) AND (@subjectId IS NULL OR h.subject_id = @subjectId)
        ORDER BY h.due_date DESC, h.id DESC
        """, new { classId, subjectId }, ct);

    public Task<List<StudentHomeworkRow>> ForStudentAsync(int classId, int studentId, CancellationToken ct = default) => QueryAsync<StudentHomeworkRow>("""
        SELECT TOP 100 h.id AS Id, h.class_id AS ClassId, h.subject_id AS SubjectId, h.title AS Title, h.description AS Description,
               h.assigned_on AS AssignedOn, h.due_date AS DueDate, h.max_marks AS MaxMarks, h.created_by_name AS CreatedByName,
               c.name + '-' + c.section AS ClassName, s.name AS SubjectName, x.status AS Status, x.marks AS Marks, x.remarks AS Remarks
        FROM homework h
        JOIN classes c ON c.id = h.class_id AND c.delstatus = 0
        JOIN subjects s ON s.id = h.subject_id AND s.delstatus = 0
        OUTER APPLY (SELECT TOP 1 status, marks, remarks FROM homework_submissions
                     WHERE homework_id = h.id AND student_id = @studentId AND delstatus = 0) x
        WHERE h.delstatus = 0 AND h.class_id = @classId
        ORDER BY h.due_date DESC
        """, new { classId, studentId }, ct);
}

public sealed class ClassSubjectQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IClassSubjectQueryRepository
{
    public Task<List<IClassSubjectQueryRepository.ClassSubjectRow>> ListAsync(CancellationToken ct = default) =>
        QueryAsync<IClassSubjectQueryRepository.ClassSubjectRow>("""
        SELECT cs.id AS Id, cs.class_id AS ClassId, cs.subject_id AS SubjectId, cs.staff_id AS StaffId, cs.component AS Component,
               cs.is_elective AS IsElective, cs.elective_group AS ElectiveGroup, cs.periods_per_week AS PeriodsPerWeek,
               cs.practical_share AS PracticalShare, c.name + '-' + c.section AS ClassName, s.name AS SubjectName,
               t.first_name + ' ' + t.last_name AS TeacherName
        FROM class_subjects cs
        JOIN classes c ON c.id = cs.class_id AND c.delstatus = 0
        JOIN subjects s ON s.id = cs.subject_id AND s.delstatus = 0
        LEFT JOIN staff t ON t.id = cs.staff_id AND t.delstatus = 0
        WHERE cs.delstatus = 0
        ORDER BY cs.class_id, cs.subject_id
        """, null, ct);
}

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

public sealed class SyllabusQueryRepository(IDapperContext dapper) : QueryRepository(dapper), ISyllabusQueryRepository
{
    public Task<List<ProgressRow>> ProgressAsync(DateOnly today, CancellationToken ct = default) => QueryAsync<ProgressRow>("""
        SELECT t.class_id AS ClassId, t.subject_id AS SubjectId, c.name + '-' + c.section AS ClassName, s.name AS SubjectName,
               COUNT(*) AS Total,
               SUM(CASE WHEN t.status = 'completed' THEN 1 ELSE 0 END) AS Completed,
               SUM(CASE WHEN t.status = 'in_progress' THEN 1 ELSE 0 END) AS InProgress,
               SUM(CASE WHEN t.status <> 'completed' AND t.planned_date < @today THEN 1 ELSE 0 END) AS Behind
        FROM syllabus_topics t
        JOIN classes c ON c.id = t.class_id AND c.delstatus = 0
        JOIN subjects s ON s.id = t.subject_id AND s.delstatus = 0
        WHERE t.delstatus = 0
        GROUP BY t.class_id, t.subject_id, c.name, c.section, s.name
        """, new { today }, ct);
}
