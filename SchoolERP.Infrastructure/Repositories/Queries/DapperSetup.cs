using System.Data;
using Dapper;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Repositories.Queries;

/// <summary>Dapper settings, applied once at start-up.</summary>
public static class DapperSetup
{
    private static bool _done;

    public static void Configure()
    {
        if (_done) return;
        _done = true;
        DefaultTypeMap.MatchNamesWithUnderscores = true; // class_teacher_id fills ClassTeacherId
        SqlMapper.AddTypeHandler(new DateOnlyHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyHandler());
    }

    /// <summary>SQL date columns to and from DateOnly, as EF Core maps them.</summary>
    private sealed class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value) => value switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            _ => DateOnly.Parse(value.ToString()!, System.Globalization.CultureInfo.InvariantCulture),
        };
    }

    private sealed class NullableDateOnlyHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value is { } d ? d.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
        }

        public override DateOnly? Parse(object value) => value is null or DBNull ? null : new DateOnlyHandler().Parse(value);
    }
}
