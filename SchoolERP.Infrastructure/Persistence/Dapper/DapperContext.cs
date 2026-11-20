using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Persistence.Dapper;

/// <summary>A new SQL Server connection on ConnectionStrings:DefaultConnection, the same database EF Core uses.</summary>
public sealed class DapperContext(IConfiguration configuration) : IDapperContext
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not set.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
