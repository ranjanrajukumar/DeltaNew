namespace SchoolERP.Domain.Common;

/// <summary>A table with an updated-at timestamp set on every save.</summary>
public interface IHasUpdatedAt
{
    DateTime UpdatedAt { get; set; }
}
