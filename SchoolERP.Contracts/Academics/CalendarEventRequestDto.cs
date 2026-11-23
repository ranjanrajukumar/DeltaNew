namespace SchoolERP.Contracts.Academics;

public sealed record CalendarEventRequestDto(string? Title, string? Type, DateOnly? StartDate, DateOnly? EndDate, string? Audience, string? Description);
