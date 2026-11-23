using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Exams;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Co-scholastic grading and the holistic progress card (the Node app's progress/progress.service.js).</summary>
public interface IProgressService
{
    Task<List<CoScholasticArea>> ListAreasAsync(CancellationToken ct = default);

    Task<int> CreateAreaAsync(Body b, CancellationToken ct = default);

    Task DeleteAreaAsync(int id, CancellationToken ct = default);

    Task<object> GradesAsync(int groupId, CancellationToken ct = default);

    Task<int> SaveGradesAsync(int groupId, JsonNode? recordsNode, CancellationToken ct = default);

    Task<object> ProgressCardAsync(int groupId, int studentId, CancellationToken ct = default);
}
