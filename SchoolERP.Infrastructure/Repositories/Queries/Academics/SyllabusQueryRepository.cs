using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using static SchoolERP.Application.Interfaces.Repositories.IHomeworkQueryRepository;
using static SchoolERP.Application.Interfaces.Repositories.ISyllabusQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

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
