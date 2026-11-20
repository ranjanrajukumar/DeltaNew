using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Academic sessions; exactly one is current.</summary>
public interface IAcademicYearService
{
    Task<List<AcademicYear>> ListAsync(CancellationToken ct = default);

    Task<int> CreateAsync(Body body, CancellationToken ct = default);

    Task UpdateAsync(int id, Body body, CancellationToken ct = default);

    Task SetCurrentAsync(int id, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
