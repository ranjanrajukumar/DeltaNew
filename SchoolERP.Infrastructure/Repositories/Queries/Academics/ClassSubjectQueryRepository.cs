using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IHomeworkQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ISyllabusQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

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
