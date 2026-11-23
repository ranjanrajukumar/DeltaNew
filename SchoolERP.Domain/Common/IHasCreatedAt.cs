namespace SchoolERP.Domain.Common;

/// <summary>A table with a created-at timestamp set on insert.</summary>
public interface IHasCreatedAt
{
    DateTime CreatedAt { get; set; }
}
