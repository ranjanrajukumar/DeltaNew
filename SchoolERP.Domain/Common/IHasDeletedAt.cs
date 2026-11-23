namespace SchoolERP.Domain.Common;

/// <summary>A table that soft-deletes its own way (a deleted-at column with a restore), mirrored into the audit columns.</summary>
public interface IHasDeletedAt
{
    DateTime? DeletedAt { get; set; }
}
