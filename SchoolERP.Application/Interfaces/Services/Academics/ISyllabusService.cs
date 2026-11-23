using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Syllabus topics and how far each class has got.</summary>
public interface ISyllabusService
{
    Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    Task<List<object>> ProgressAsync(CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
