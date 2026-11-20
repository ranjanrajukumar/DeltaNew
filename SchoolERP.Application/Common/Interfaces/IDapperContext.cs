using System.Data;

namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// Raw SQL with Dapper, beside EF Core, for read-heavy queries (reports, searches, dashboards).
/// Open with <c>using var connection = dapper.CreateConnection();</c> then call Dapper's QueryAsync/ExecuteAsync on it.
/// </summary>
public interface IDapperContext
{
    IDbConnection CreateConnection();
}
