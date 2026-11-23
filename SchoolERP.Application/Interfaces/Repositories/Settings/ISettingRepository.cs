using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>School-wide settings rows (EF Core). A setting is keyed by its text key, not an id.</summary>
public interface ISettingRepository : IRepository<Setting>
{
    /// <summary>Every saved setting, read-only.</summary>
    Task<List<Setting>> AllAsync(CancellationToken ct = default);

    /// <summary>The row with this key, read-only; null when not saved.</summary>
    Task<Setting?> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>The row with this key, tracked; null when not saved.</summary>
    Task<Setting?> FindByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>Every row with this key, tracked (for removing them).</summary>
    Task<List<Setting>> ListByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>A row's raw value (JSON text), or null.</summary>
    Task<string?> RawValueAsync(string key, CancellationToken ct = default);
}
