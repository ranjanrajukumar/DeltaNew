using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Sent email and SMS, and the absence facts an absence SMS depends on (EF Core).</summary>
public interface IMessageRepository : IRepository<Message>
{
    /// <summary>Whether the student is marked absent on this day.</summary>
    Task<bool> IsMarkedAbsentAsync(int studentId, DateOnly day, CancellationToken ct = default);

    /// <summary>Whether an SMS absence alert was already logged for this student and day.</summary>
    Task<bool> AbsenceSmsLoggedAsync(int studentId, DateOnly day, CancellationToken ct = default);

    /// <summary>The student with their class, read-only; throws when the student does not exist.</summary>
    Task<Student> StudentWithClassAsync(int studentId, CancellationToken ct = default);

    void AddAbsenceAlert(AbsenceAlert alert);
}
