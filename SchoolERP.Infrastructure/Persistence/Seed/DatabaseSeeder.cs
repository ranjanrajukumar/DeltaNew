using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Modules.Boards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string Section = "Seed";

    /// <summary>Apply pending EF Core migrations at startup (the Node app synced its schema on every start).</summary>
    public bool MigrateOnStartup { get; set; } = true;

    public string AdminEmail { get; set; } = "admin@school.local";

    /// <summary>The first administrator's password; change it after the first sign-in.</summary>
    public string AdminPassword { get; set; } = "admin123";
}

/// <summary>
/// First-run data the Node app's config/migration.js created: the initial administrator, the two boards every class needs,
/// their starter rules, leave types and LMS tags. Safe to run on every start: nothing that exists is touched. The other starter
/// data (masters, role presets, report starters, app menu) is seeded as their modules are ported.
/// </summary>
public sealed class DatabaseSeeder(
    SchoolErpDbContext db,
    IPasswordHasher hasher,
    IOptions<SeedOptions> options,
    ILogger<DatabaseSeeder> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        if (options.Value.MigrateOnStartup)
            await db.Database.MigrateAsync(ct);

        await SeedAdminAsync(ct);
        await SeedBoardsAsync(ct);
        await SeedBoardRulesAsync(ct);
        await SeedLeaveTypesAsync(ct);
        await SeedLmsTagsAsync(ct);
    }

    /// <summary>
    /// Fills each missing board rule with its starter value, saved as unverified (the Node app's ensureBoardData). A rule that
    /// exists in any version is never touched. CBSE's grading carries over the single grade scale schools saved before scales
    /// were per board.
    /// </summary>
    private async Task SeedBoardRulesAsync(CancellationToken ct)
    {
        var added = 0;
        foreach (var name in BoardStarters.BoardNames)
        {
            var board = await db.Set<Board>().FirstOrDefaultAsync(b => b.Name == name, ct);
            if (board is null) continue;
            var starters = new List<(string Type, string Scope, JsonObject Payload, bool Carried)>();
            foreach (var stage in Application.Modules.Academics.SubjectTemplates.Stages)
                starters.Add(("subject_template", stage, BoardStarters.SubjectTemplate(name, stage), false));
            starters.Add(("exam_pattern", "default", BoardStarters.ExamPattern(name), false));
            starters.Add(("promotion", "default", BoardStarters.Promotion(), false));
            starters.Add(("report_card", "default", BoardStarters.ReportCard(), false));
            var legacy = name == "CBSE" ? await LegacyGradingAsync(ct) : null;
            starters.Add(("grading", "default", legacy ?? BoardStarters.Grading(name), legacy is not null));

            foreach (var (type, scope, payload, carried) in starters)
            {
                if (await db.Set<BoardRule>().AnyAsync(r => r.BoardId == board.Id && r.RuleType == type && r.ScopeKey == scope, ct)) continue;
                db.Add(new BoardRule
                {
                    BoardId = board.Id, RuleType = type, ScopeKey = scope, Payload = payload.ToJsonString(),
                    ChangeNote = carried ? "Carried over from the earlier school-wide grade setup" : BoardStarters.StarterNote,
                    CreatedByName = "system",
                });
                added++;
            }
        }
        if (added == 0) return;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} starter board rules (unverified).", added);
    }

    /// <summary>The single grade scale schools saved before scales were per board; it becomes CBSE's.</summary>
    private async Task<JsonObject?> LegacyGradingAsync(CancellationToken ct)
    {
        var bands = await db.Set<GradeBand>().AsNoTracking().Where(b => b.Board == "CBSE").OrderByDescending(b => b.MinPercent).ToListAsync(ct);
        if (bands.Count == 0) return null;
        var setting = await db.Set<GradingSetting>().AsNoTracking().FirstOrDefaultAsync(g => g.Id == 1, ct);
        var list = new JsonArray();
        foreach (var b in bands) list.Add(new JsonObject { ["grade"] = b.Grade, ["min"] = b.MinPercent });
        return new JsonObject { ["passPercent"] = setting is null ? 33 : setting.PassPercent, ["bands"] = list };
    }

    /// <summary>The leave types a new school starts with (the Node app's ensureLeaveTypes).</summary>
    private async Task SeedLeaveTypesAsync(CancellationToken ct)
    {
        if (await db.Set<LeaveType>().AnyAsync(ct)) return;
        db.AddRange(
            new LeaveType { Name = "Casual", AppliesTo = "staff", YearlyDays = 12 },
            new LeaveType { Name = "Sick", AppliesTo = "both", YearlyDays = null, Approver = "class_teacher" },
            new LeaveType { Name = "Earned", AppliesTo = "staff", YearlyDays = 15 },
            new LeaveType { Name = "Unpaid", AppliesTo = "staff", YearlyDays = null, Paid = false },
            new LeaveType { Name = "Family function", AppliesTo = "student", YearlyDays = null, Approver = "class_teacher" });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>The LMS tags a new school starts with (the Node app's ensureLmsTags).</summary>
    private async Task SeedLmsTagsAsync(CancellationToken ct)
    {
        if (await db.Set<LmsTag>().AnyAsync(ct)) return;
        db.AddRange(new[] { "Maths", "Science", "English", "Reading", "Revision", "Homework", "Exam prep", "Project" }
            .Select(name => new LmsTag { Name = name }));
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        if (await db.Set<User>().IgnoreQueryFilters().AnyAsync(ct)) return;
        db.Add(new User
        {
            Name = "Admin",
            Email = options.Value.AdminEmail,
            PasswordHash = hasher.Hash(options.Value.AdminPassword),
            Role = "admin",
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded the initial administrator account ({Email}).", options.Value.AdminEmail);
    }

    private async Task SeedBoardsAsync(CancellationToken ct)
    {
        foreach (var (code, name) in new[] { ("CBSE", "CBSE"), ("UP", "UP Board") })
            if (!await db.Set<Board>().AnyAsync(b => b.Name == name, ct))
                db.Add(new Board { Code = code, Name = name });
        await db.SaveChangesAsync(ct);

        // every class points at its board (by name), and a class with none follows CBSE
        var boards = await db.Set<Board>().ToListAsync(ct);
        var cbse = boards.First(b => b.Name == "CBSE");
        foreach (var cls in await db.Set<SchoolClass>().Where(c => c.BoardId == null).ToListAsync(ct))
        {
            var board = boards.FirstOrDefault(b => b.Name == cls.Board) ?? cbse;
            cls.BoardId = board.Id;
            cls.Board = board.Name;
        }
        await db.SaveChangesAsync(ct);
    }
}
