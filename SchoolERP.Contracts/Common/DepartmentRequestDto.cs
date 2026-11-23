namespace SchoolERP.Contracts.Common;

/// <summary>Allowed write fields for a department.</summary>
public sealed record DepartmentRequestDto(string? Name, int? HeadStaffId);
