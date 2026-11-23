using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Co-scholastic areas and grades (EF Core).</summary>
public interface ICoScholasticRepository : IRepository<CoScholasticArea>
{
    /// <summary>Areas by category, then name.</summary>
    Task<List<CoScholasticArea>> ListAreasAsync(CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    /// <summary>The tracked areas with this id (zero or one).</summary>
    Task<List<CoScholasticArea>> FindAllAsync(int id, CancellationToken ct = default);

    Task<List<int>> AreaIdsAsync(CancellationToken ct = default);

    /// <summary>Every grade given in a group (read-only).</summary>
    Task<List<CoScholasticGrade>> GradesOfGroupAsync(int groupId, CancellationToken ct = default);

    /// <summary>One student's grades in a group (read-only).</summary>
    Task<List<CoScholasticGrade>> GradesOfStudentAsync(int studentId, int groupId, CancellationToken ct = default);

    /// <summary>The tracked grade of a student in an area and group, or null.</summary>
    Task<CoScholasticGrade?> FindGradeAsync(int studentId, int areaId, int groupId, CancellationToken ct = default);

    void AddGrade(CoScholasticGrade grade);

    void RemoveGrade(CoScholasticGrade grade);
}
