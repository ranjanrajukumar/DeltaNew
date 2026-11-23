using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class CoScholasticRepository(ApplicationDbContext db) : Repository<CoScholasticArea>(db), ICoScholasticRepository
{
    private DbSet<CoScholasticGrade> Grades => Db.Set<CoScholasticGrade>();

    public Task<List<CoScholasticArea>> ListAreasAsync(CancellationToken ct = default) =>
        Read.OrderBy(a => a.Category).ThenBy(a => a.Name).ToListAsync(ct);

    public Task<bool> NameExistsAsync(string name, CancellationToken ct = default) => Set.AnyAsync(a => a.Name == name, ct);

    public Task<List<CoScholasticArea>> FindAllAsync(int id, CancellationToken ct = default) => Set.Where(a => a.Id == id).ToListAsync(ct);

    public Task<List<int>> AreaIdsAsync(CancellationToken ct = default) => Set.Select(a => a.Id).ToListAsync(ct);

    public Task<List<CoScholasticGrade>> GradesOfGroupAsync(int groupId, CancellationToken ct = default) =>
        Grades.AsNoTracking().Where(g => g.ExamGroupId == groupId).ToListAsync(ct);

    public Task<List<CoScholasticGrade>> GradesOfStudentAsync(int studentId, int groupId, CancellationToken ct = default) =>
        Grades.AsNoTracking().Where(g => g.StudentId == studentId && g.ExamGroupId == groupId).ToListAsync(ct);

    public Task<CoScholasticGrade?> FindGradeAsync(int studentId, int areaId, int groupId, CancellationToken ct = default) =>
        Grades.FirstOrDefaultAsync(g => g.StudentId == studentId && g.AreaId == areaId && g.ExamGroupId == groupId, ct);

    public void AddGrade(CoScholasticGrade grade) => Grades.Add(grade);

    public void RemoveGrade(CoScholasticGrade grade) => Grades.Remove(grade);
}
