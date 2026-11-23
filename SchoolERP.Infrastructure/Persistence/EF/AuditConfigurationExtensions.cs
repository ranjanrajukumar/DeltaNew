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
