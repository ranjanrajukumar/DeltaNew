using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;
using SchoolERP.Contracts.Academics;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Academic sessions; exactly one is current.</summary>
public interface IAcademicYearService
{
    Task<List<AcademicYearResponse>> ListAsync(CancellationToken ct = default);

    Task<int> CreateAsync(AcademicYearRequest request, CancellationToken ct = default);

    Task UpdateAsync(int id, AcademicYearRequest request, CancellationToken ct = default);

    Task SetCurrentAsync(int id, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}
