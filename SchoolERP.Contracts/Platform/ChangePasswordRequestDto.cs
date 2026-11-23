namespace SchoolERP.Contracts.Platform;

public sealed record ChangePasswordRequestDto(string? CurrentPassword, string? NewPassword);
