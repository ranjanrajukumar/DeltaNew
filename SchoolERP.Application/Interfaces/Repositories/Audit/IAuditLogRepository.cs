using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>The audit trail (EF Core; written once, never changed).</summary>
public interface IAuditLogRepository : IRepository<AuditLog>;
