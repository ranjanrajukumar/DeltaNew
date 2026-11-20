using SchoolERP.Application.Modules.Academics;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Which subjects a class studies and which electives each student took.</summary>
public interface ISubjectMapService
{
    Task<ClassMap> ClassSubjectMapAsync(int classId, CancellationToken ct = default);

    /// <summary>Saves a student's electives (each an elective of their class, one per group). Returns how many were saved.</summary>
    Task<int> SetStudentElectivesAsync(int studentId, IEnumerable<int> classSubjectIds, CancellationToken ct = default);

    /// <summary>Removes elective choices that no longer match each student's class.</summary>
    Task PruneElectivesAsync(IEnumerable<int> studentIds, CancellationToken ct = default);

    Task<object> StudentSubjectsAsync(int studentId, CancellationToken ct = default);

    /// <summary>After a subject stops being an elective, nobody holds a choice of it any more.</summary>
    Task AfterClassSubjectChangeAsync(int id, CancellationToken ct = default);

    Task<object> ElectiveSheetAsync(int classId, CancellationToken ct = default);

    Task<ElectiveFacts> ElectiveFactsAsync(int classId, CancellationToken ct = default);
}
