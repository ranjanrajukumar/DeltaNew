using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>A record's detailed particulars in record_extras (EF Core).</summary>
public interface IRecordExtraRepository : IRepository<RecordExtra>
{
    /// <summary>The particulars row of (entity, id), read-only; null when none.</summary>
    Task<RecordExtra?> GetForAsync(string entity, int entityId, CancellationToken ct = default);

    /// <summary>The particulars row of (entity, id), tracked; null when none.</summary>
    Task<RecordExtra?> FindForAsync(string entity, int entityId, CancellationToken ct = default);
}
