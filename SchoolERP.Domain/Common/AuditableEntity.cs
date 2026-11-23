namespace SchoolERP.Domain.Common;

/// <summary>
/// The seven audit columns every table carries (same as the Node app's auditSoftDelete.js):
///   authadd / addondt    who created the row and when
///   authedit / editondt  who last changed it and when
///   authdel / delondt    who deleted it and when
///   delstatus            true = deleted. A delete only sets this (a soft delete); every read ignores deleted rows.
/// They are filled in by the DbContext on save, so no service has to set them.
/// </summary>
public abstract class AuditableEntity
{
    public string? Authadd { get; set; }
    public string? Authedit { get; set; }
    public string? Authdel { get; set; }
    public DateTime? Addondt { get; set; }
    public DateTime? Editondt { get; set; }
    public DateTime? Delondt { get; set; }
    public bool Delstatus { get; set; }
}
