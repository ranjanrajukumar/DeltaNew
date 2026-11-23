using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Students as the academics services read them (EF Core).</summary>
public interface IStudentLookupRepository
{
    /// <summary>Active students of a class, by first name.</summary>
    Task<List<Student>> ActiveInClassAsync(int classId, CancellationToken ct = default);
}
