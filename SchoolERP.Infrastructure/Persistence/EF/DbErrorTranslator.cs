using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;

namespace SchoolERP.Infrastructure.Persistence;

/// <summary>
/// Turns database failures into the answers the Node app gave (middleware/errorMiddleware.js): a duplicate is
/// "&lt;field&gt; must be unique" (400), a missing parent 400, a still-referenced parent 409, any other database error 400
/// without table or column names. Returns null for errors that are not database errors.
/// </summary>
public sealed partial class DbErrorTranslator(ApplicationDbContext db)
{
    public (int Status, string Message, string? LogDetail)? Translate(Exception ex)
    {
        if (ex is StillReferencedException still)
            return (409, "This record is still used by other records, so it cannot be deleted.", still.Message);

        var sql = ex as SqlException ?? ex.InnerException as SqlException ?? (ex as DbUpdateException)?.InnerException as SqlException;
        if (sql is null) return ex is DbUpdateException ? (400, "Invalid value in the request", ex.Message) : null;

        switch (sql.Number)
        {
            case 2601: // duplicate key in a unique index
            case 2627: // unique constraint
                return (400, UniqueMessage(sql.Message), sql.Message);
            case 547: // FK or CHECK violation
                if (sql.Message.Contains("CHECK constraint", StringComparison.OrdinalIgnoreCase))
                    return (400, "Invalid value in the request", sql.Message);
                return sql.Message.Contains("DELETE statement", StringComparison.OrdinalIgnoreCase)
                    ? (409, "This record is still used by other records, so it cannot be deleted.", sql.Message)
                    : (400, "A record this refers to does not exist.", sql.Message);
            default:
                return (400, "Invalid value in the request", sql.Message);
        }
    }

    /// <summary>Sequelize said "admissionNo must be unique"; find the index's properties to say the same.</summary>
    private string UniqueMessage(string sqlMessage)
    {
        var match = IndexName().Match(sqlMessage);
        if (match.Success)
        {
            var name = match.Groups[1].Value;
            foreach (var entity in db.Model.GetEntityTypes())
            {
                var index = entity.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == name);
                if (index is null) continue;
                var fields = string.Join(", ", index.Properties.Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name)));
                return $"{fields} must be unique";
            }
        }
        return "A record with this value already exists";
    }

    [GeneratedRegex(@"(?:unique index|constraint) '([^']+)'")]
    private static partial Regex IndexName();
}
