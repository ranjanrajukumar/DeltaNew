using System.Data;
using Dapper;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Repositories.Queries;

/// <summary>
/// Base of the Dapper query repositories: complex reads (lists with joins and counts, overviews, reports) written as SQL.
/// Writes and simple reads stay in the EF Core repositories.
/// <para>
/// SQL here must do what EF Core's query filters did automatically: every table read must skip soft-deleted rows
/// (<c>delstatus = 0</c>), including tables that are only joined. Exceptions: <c>admissions</c> uses <c>deleted_at IS NULL</c>,
/// and <c>audit_logs</c>, <c>login_events</c>, <c>push_subscriptions</c> and <c>photos</c> are hard-deleted (no filter).
/// Reads run on their own connection, so do not call a query
/// repository inside a transaction expecting to see rows that transaction has written but not committed.
/// </para>
/// </summary>
public abstract class QueryRepository(IDapperContext dapper)
{
    protected async Task<List<T>> QueryAsync<T>(string sql, object? args, CancellationToken ct)
    {
        using var connection = dapper.CreateConnection();
        return (await connection.QueryAsync<T>(new CommandDefinition(sql, args, cancellationToken: ct))).AsList();
    }

    protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? args, CancellationToken ct)
    {
        using var connection = dapper.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql, args, cancellationToken: ct));
    }

    protected async Task<T> ScalarAsync<T>(string sql, object? args, CancellationToken ct)
    {
        using var connection = dapper.CreateConnection();
        return (await connection.ExecuteScalarAsync<T>(new CommandDefinition(sql, args, cancellationToken: ct)))!;
    }

    /// <summary>Several result sets in one round trip.</summary>
    protected async Task<TResult> QueryMultipleAsync<TResult>(string sql, object? args, Func<SqlMapper.GridReader, Task<TResult>> read, CancellationToken ct)
    {
        using var connection = dapper.CreateConnection();
        using var grid = await connection.QueryMultipleAsync(new CommandDefinition(sql, args, cancellationToken: ct));
        return await read(grid);
    }
}
