using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Homework and who handed it in (the Node app's academics/homework.service.js).</summary>
public interface IHomeworkService
{
    Task<List<JsonObject>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<object> SubmissionsAsync(int id, CancellationToken ct = default);

    Task<int> SaveSubmissionsAsync(int id, JsonNode? records, CancellationToken ct = default);

    /// <summary>A child's view: homework for their class with their own submission status.</summary>
    Task<List<JsonObject>> ForStudentAsync(Student student, CancellationToken ct = default);
}
