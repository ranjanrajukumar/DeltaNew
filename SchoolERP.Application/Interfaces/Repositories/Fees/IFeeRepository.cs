using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Student fees and fee structures (EF Core).</summary>
public interface IFeeRepository : IRepository<Fee>
{
    /// <summary>Which of these students already have a fee with this title.</summary>
    Task<List<int>> StudentsBilledForAsync(string title, IReadOnlyCollection<int> studentIds, CancellationToken ct = default);

    /// <summary>A class's fee structure items, read-only.</summary>
    Task<List<FeeStructure>> StructureOfClassAsync(int classId, CancellationToken ct = default);
}
