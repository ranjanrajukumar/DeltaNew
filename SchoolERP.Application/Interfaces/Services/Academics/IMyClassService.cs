using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>A class teacher's own class at a glance.</summary>
public interface IMyClassService
{
    /// <summary>Roll, today's attendance and leave waiting for a decision; null when the class does not exist.</summary>
    Task<object?> MyClassAsync(int classId, CancellationToken ct = default);
}
