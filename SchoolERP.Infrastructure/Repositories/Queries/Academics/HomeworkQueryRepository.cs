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
