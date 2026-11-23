using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.ISubjectMapQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class SubjectMapQueryRepository(IDapperContext dapper) : QueryRepository(dapper), ISubjectMapQueryRepository
{
    public Task<List<ClassSubjectRow>> ClassSubjectsAsync(int classId, CancellationToken ct = default) => QueryAsync<ClassSubjectRow>("""
        SELECT cs.id AS Id, cs.subject_id AS SubjectId, s.name AS Name, s.code AS Code, cs.component AS Component,
               cs.is_elective AS IsElective, cs.elective_group AS ElectiveGroup, cs.periods_per_week AS PeriodsPerWeek,
               cs.practical_share AS PracticalShare, cs.staff_id AS StaffId, t.first_name + ' ' + t.last_name AS TeacherName,
               (SELECT COUNT(*) FROM student_subjects ss WHERE ss.class_subject_id = cs.id AND ss.delstatus = 0) AS Choosers
        FROM class_subjects cs
        JOIN subjects s ON s.id = cs.subject_id AND s.delstatus = 0
        LEFT JOIN staff t ON t.id = cs.staff_id AND t.delstatus = 0
        WHERE cs.class_id = @classId AND cs.delstatus = 0
        ORDER BY cs.is_elective, cs.id
        """, new { classId }, ct);
}
