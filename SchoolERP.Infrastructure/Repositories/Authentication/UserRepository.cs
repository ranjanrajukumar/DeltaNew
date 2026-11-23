using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;
using AttendanceMark = SchoolERP.Domain.Entities.Attendance;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class UserRepository(ApplicationDbContext db) : Repository<User>(db), IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => Read.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByAdmissionNoAsync(string admissionNo, CancellationToken ct = default)
    {
        var studentId = await Db.Set<Student>().Where(s => s.AdmissionNo == admissionNo).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
        if (studentId is null) return null;
        var userId = await Db.Set<StudentAccount>().Where(a => a.StudentId == studentId).Select(a => (int?)a.UserId).FirstOrDefaultAsync(ct);
        return userId is null ? null : await Read.FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public Task<User?> GetWithRoleAsync(int id, CancellationToken ct = default) =>
        Read.Include(u => u.RoleNavigation).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User> FindExistingAsync(int id, CancellationToken ct = default) => Set.FirstAsync(u => u.Id == id, ct);

    public Task<List<int>> IdsWithRolesAsync(IReadOnlyCollection<string>? roles, CancellationToken ct = default) =>
        (roles is null ? Set : Set.Where(u => roles.Contains(u.Role))).Select(u => u.Id).ToListAsync(ct);
}
