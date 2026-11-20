using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence;

internal static class AuditConfigurationExtensions
{
    /// <summary>The seven audit columns, with the Node app's column names and types.</summary>
    public static void ConfigureAudit<T>(this EntityTypeBuilder<T> builder) where T : AuditableEntity
    {
        builder.Property(e => e.Authadd).HasColumnName("authadd").HasMaxLength(100);
        builder.Property(e => e.Authedit).HasColumnName("authedit").HasMaxLength(100);
        builder.Property(e => e.Authdel).HasColumnName("authdel").HasMaxLength(100);
        builder.Property(e => e.Addondt).HasColumnName("addondt").HasColumnType("datetime2");
        builder.Property(e => e.Editondt).HasColumnName("editondt").HasColumnType("datetime2");
        builder.Property(e => e.Delondt).HasColumnName("delondt").HasColumnType("datetime2");
        builder.Property(e => e.Delstatus).HasColumnName("delstatus").HasDefaultValue(false);
    }
}

/// <summary>JSON columns (MySQL JSON in the Node app) are nvarchar(max) here and a JsonNode in C#, so the API still returns them as objects.</summary>
internal static class JsonNodeConversion
{
    public static readonly ValueConverter<JsonNode?, string?> Converter = new(
        v => v == null ? null : v.ToJsonString((System.Text.Json.JsonSerializerOptions?)null),
        v => v == null ? null : JsonNode.Parse(v, null, default));

    public static readonly ValueComparer<JsonNode?> Comparer = new(
        (a, b) => JsonNode.DeepEquals(a, b),
        v => v == null ? 0 : v.ToJsonString((System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
        v => v == null ? null : v.DeepClone());
}

/// <summary>What deleting a parent does to a child row (the Node associations' onDelete).</summary>
internal enum DeleteRule
{
    CASCADE,
    SET_NULL,
    RESTRICT,
}

/// <param name="ChildType">The child entity.</param>
/// <param name="ChildTable">Its table.</param>
/// <param name="ForeignKeyColumn">The column on the child pointing at the parent's id.</param>
/// <param name="Rule">What a delete of the parent does.</param>
/// <param name="ChildHardDeletes">The child table keeps real deletes (no delstatus filter).</param>
internal sealed record ChildRule(Type ChildType, string ChildTable, string ForeignKeyColumn, DeleteRule Rule, bool ChildHardDeletes);
