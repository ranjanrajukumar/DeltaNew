using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Exams;

/// <summary>Grade setup, the board exam pattern, exam groups, merit lists and revaluation (the Node app's exams/examExtras.service.js).</summary>
public sealed class ExamExtrasService(IApplicationDbContext db, ExamsService exams, GradingService grading, IBoardService boards, ICurrentUser user)
{
    private static string? ClassNameOf(SchoolClass? c) => c is null ? null : $"{c.Name}-{c.Section}";

    /* ---------- Grade setup ---------- */

    /// <summary>Saves a board's grade scale and pass mark as a new version of its grading rule.</summary>
    public Task SaveGradingAsync(JsonObject? json, CancellationToken ct = default)
    {
        var b = new Body(json);
        var payload = new JsonObject { ["passPercent"] = json?["passPercent"]?.DeepClone(), ["bands"] = json?["bands"]?.DeepClone() };
        return boards.SaveRuleAsync(b.Raw("board")?.DeepClone() ?? "CBSE", "grading", "default", null, payload,
            b.Str("sourceRef"), b.Raw("changeNote") is null ? "Edited in Grade setup" : b.Str("changeNote"), user.Name, ct);
    }

    public async Task<object> GradingJsonAsync(string? board, CancellationToken ct = default)
    {
        var g = await grading.LoadAsync(board, ct);
        return new { passPercent = g.PassPercent, bands = g.BandsJson(), isDefault = g.IsDefault, version = g.Version, sourceRef = g.SourceRef };
    }

    /* ---------- Board exam pattern ---------- */

    /// <summary>
    /// Gives each class its board's yearly exams and the result group that combines them by weight. A class that already has the
    /// group is left alone, as is any exam name it already has.
    /// </summary>
    public async Task<object> ApplyExamPatternAsync(JsonNode? classIdsNode, CancellationToken ct = default)
    {
        var ids = classIdsNode is JsonArray a ? a.Select(n => Body.AsNumber(n) is { } x ? (int)x : 0).ToList() : [];
        var classes = await db.Set<SchoolClass>().AsNoTracking().Where(c => ids.Count == 0 || ids.Contains(c.Id)).ToListAsync(ct);
        if (classes.Count == 0) throw HttpException.BadRequest("No classes to apply a pattern to");
        int examCount = 0, groupCount = 0;
        var skipped = new List<string>();
        var rules = new Dictionary<int, JsonNode?>(); // one lookup per board
        await db.InTransactionAsync(async () =>
        {
            foreach (var cls in classes)
            {
                var label = $"{cls.Name}-{cls.Section}";
                var board = await boards.OfClassAsync(cls, ct) ?? throw HttpException.Conflict("Add a board first");
                if (!rules.TryGetValue(board.Id, out var pattern))
                    rules[board.Id] = pattern = (await boards.RequireRuleAsync(board, "exam_pattern", ct: ct)).Payload;
                var groupName = Body.AsString(pattern?["groupName"]) ?? "";
                var skip = (pattern?["skipStages"] as JsonArray ?? []).Select(Body.AsString).ToList();
                if (skip.Contains(SubjectTemplates.StageOf(cls.Name)))
                {
                    skipped.Add($"{label} (no written exams)");
                    continue;
                }
                if (await db.Set<ExamGroup>().AnyAsync(g => g.ClassId == cls.Id && g.Name == groupName, ct))
                {
                    skipped.Add($"{label} (already has {groupName})");
                    continue;
                }
                var group = new ExamGroup { ClassId = cls.Id, Name = groupName };
                db.Set<ExamGroup>().Add(group);
                await db.SaveChangesAsync(ct);
                groupCount++;
                var have = (await db.Set<Exam>().Where(e => e.ClassId == cls.Id).Select(e => e.Name).ToListAsync(ct)).ToHashSet();
                foreach (var p in pattern?["exams"] as JsonArray ?? [])
                {
                    var name = Body.AsString(p?["name"]) ?? "";
                    if (have.Contains(name)) continue;
                    db.Set<Exam>().Add(new Exam
                    {
                        Name = name, ClassId = cls.Id, MaxMarks = (int)(Body.AsNumber(p?["maxMarks"]) ?? 100),
                        Weight = (int)(Body.AsNumber(p?["weight"]) ?? 100), ExamGroupId = group.Id,
                    });
                    examCount++;
                }
                await db.SaveChangesAsync(ct);
            }
            return true;
        }, ct);
        return new { classes = classes.Count, groups = groupCount, exams = examCount, skipped };
    }

    /* ---------- Exam groups ---------- */

    public async Task<List<object>> ListGroupsAsync(CancellationToken ct = default)
    {
        var groups = await db.Set<ExamGroup>().AsNoTracking().Include(g => g.SchoolClass).Where(g => g.SchoolClass != null)
            .OrderByDescending(g => g.Id).ToListAsync(ct);
        var groupIds = groups.Select(g => g.Id).ToList();
        var members = await db.Set<Exam>().AsNoTracking().Where(e => e.ExamGroupId != null && groupIds.Contains(e.ExamGroupId.Value))
            .OrderBy(e => e.Id).Select(e => new { e.Id, e.Name, e.Weight, Group = e.ExamGroupId!.Value }).ToListAsync(ct);
        return groups.Select(g => (object)new
        {
            id = g.Id,
            classId = g.ClassId,
            name = g.Name,
            className = ClassNameOf(g.SchoolClass),
            exams = members.Where(e => e.Group == g.Id).Select(e => new { id = e.Id, name = e.Name, weight = e.Weight }),
        }).ToList();
    }

    public async Task<int> CreateGroupAsync(Body b, CancellationToken ct = default)
    {
        if (!b.Truthy("classId") || b.IsBlankTrimmed("name")) throw HttpException.BadRequest("Class and name are required");
        var group = new ExamGroup { ClassId = b.Int("classId") ?? throw HttpException.BadRequest("Invalid value in the request"), Name = b.Trimmed("name") };
        db.Set<ExamGroup>().Add(group);
        await db.SaveChangesAsync(ct);
        return group.Id;
    }

    public async Task DeleteGroupAsync(int id, CancellationToken ct = default)
    {
        var group = await db.Set<ExamGroup>().FirstOrDefaultAsync(g => g.Id == id, ct) ?? throw HttpException.NotFound("Exam group not found");
        db.Set<ExamGroup>().Remove(group);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Attaches an exam to a group of the same class, or detaches it when the group is empty.</summary>
    public async Task AssignExamAsync(Body b, CancellationToken ct = default)
    {
        var examId = b.Int("examId");
        var exam = examId is null ? null : await db.Set<Exam>().FirstOrDefaultAsync(e => e.Id == examId, ct);
        if (exam is null) throw HttpException.NotFound("Exam not found");
        int? groupId = b.Truthy("groupId") ? b.Int("groupId") : null;
        if (b.Truthy("groupId"))
        {
            var group = groupId is null ? null : await db.Set<ExamGroup>().AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId, ct);
            if (group is null) throw HttpException.NotFound("Exam group not found");
            if (group.ClassId != exam.ClassId) throw HttpException.BadRequest("An exam can only join a group of its own class");
        }
        exam.ExamGroupId = groupId;
        // weight !== undefined && weight !== '' (a null weight is checked, and refused)
        if (b.Has("weight") && !(b.Raw("weight") is JsonValue wv && wv.TryGetValue<string>(out var ws) && ws == ""))
        {
            if (b.Num("weight") is not { } w || w != Math.Floor(w) || w < 1 || w > 1000) throw HttpException.BadRequest("Weight must be a whole number from 1 to 1000");
            exam.Weight = (int)w;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>One student's combined result in a group.</summary>
    public sealed record GroupRow(int StudentId, string AdmissionNo, string Name, Dictionary<int, double> Parts, double Percentage, string Grade, string Result)
    {
        public int Rank { get; set; }

        public JsonObject ToJson()
        {
            var parts = new JsonObject();
            foreach (var (examId, pct) in Parts) parts[examId.ToString(CultureInfo.InvariantCulture)] = pct;
            return new JsonObject
            {
                ["studentId"] = StudentId, ["admissionNo"] = AdmissionNo, ["name"] = Name, ["parts"] = parts,
                ["percentage"] = Percentage, ["grade"] = Grade, ["result"] = Result, ["rank"] = Rank,
            };
        }
    }

    public sealed record GroupResults(ExamGroup Group, List<Exam> Exams, List<GroupRow> Rows)
    {
        public object ToJson() => new
        {
            group = new { id = Group.Id, name = Group.Name, className = ClassNameOf(Group.SchoolClass) },
            exams = Exams.Select(e => new { id = e.Id, name = e.Name, weight = e.Weight }),
            rows = Rows.Select(r => r.ToJson()),
        };
    }

    /// <summary>
    /// Combined result for a group: each student's exam percentages, averaged by the exams' weights. A student who missed an
    /// exam keeps the average of the exams they sat but is marked incomplete.
    /// </summary>
    public async Task<GroupResults> GroupResultsAsync(int groupId, CancellationToken ct = default)
    {
        var group = await db.Set<ExamGroup>().AsNoTracking().Include(g => g.SchoolClass).FirstOrDefaultAsync(g => g.Id == groupId, ct)
            ?? throw HttpException.NotFound("Exam group not found");
        var groupExams = await db.Set<Exam>().AsNoTracking().Where(e => e.ExamGroupId == groupId).OrderBy(e => e.Id).ToListAsync(ct);
        if (groupExams.Count == 0) return new GroupResults(group, [], []);

        var policy = await grading.LoadAsync(await boards.OfClassAsync(group.SchoolClass, ct) ?? throw HttpException.Conflict("Add a board first"), ct: ct);
        var students = new Dictionary<int, (string No, string Name, double Weighted, double Weights, int Taken, List<string> Results, Dictionary<int, double> Parts)>();
        var order = new List<int>();
        foreach (var exam in groupExams)
        {
            foreach (var r in (await exams.ResultsAsync(exam.Id, policy, ct)).Rows)
            {
                if (!students.TryGetValue(r.StudentId, out var s))
                {
                    s = (r.AdmissionNo, r.Name, 0, 0, 0, [], []);
                    order.Add(r.StudentId);
                }
                s.Weighted += r.Percentage * exam.Weight;
                s.Weights += exam.Weight;
                s.Taken++;
                s.Results.Add(r.Result);
                s.Parts[exam.Id] = r.Percentage;
                students[r.StudentId] = s;
            }
        }
        var rows = order.Select(id =>
        {
            var s = students[id];
            var percentage = Js.Round2(s.Weighted / s.Weights);
            var result = s.Results.Contains("fail") ? "fail" : s.Results.Contains("incomplete") || s.Taken < groupExams.Count ? "incomplete" : "pass";
            return new GroupRow(id, s.No, s.Name, s.Parts, percentage, policy.GradeFor(percentage), result);
        }).OrderByDescending(r => r.Percentage).ToList();
        for (var i = 0; i < rows.Count; i++) rows[i].Rank = i > 0 && rows[i - 1].Percentage == rows[i].Percentage ? rows[i - 1].Rank : i + 1;
        return new GroupResults(group, groupExams, rows);
    }

    /* ---------- Merit list ---------- */

    /// <summary>Passing students only, ranked afresh (ties share a rank), for one exam or one group, top <paramref name="limit"/>.</summary>
    public async Task<object> MeritListAsync(int? examId, int? groupId, double? limit, CancellationToken ct = default)
    {
        if (examId is null && groupId is null) throw HttpException.BadRequest("Choose an exam or an exam group");
        var top = Math.Min(Math.Max(limit is { } l and not 0 && !double.IsNaN(l) ? l : 10, 1), 200);
        string title;
        string? className;
        List<(JsonObject Row, double Key, string Result)> rows;
        bool byPercentage;
        if (groupId is { } gid)
        {
            var g = await GroupResultsAsync(gid, ct);
            title = g.Group.Name;
            className = ClassNameOf(g.Group.SchoolClass);
            rows = g.Rows.Select(r => (r.ToJson(), r.Percentage, r.Result)).ToList();
            byPercentage = true;
        }
        else
        {
            var r = await exams.ResultsAsync(examId!.Value, ct: ct);
            var cls = await db.Set<SchoolClass>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == r.Exam.ClassId, ct);
            title = r.Exam.Name;
            className = ClassNameOf(cls);
            rows = r.Rows.Select(x => (x.ToJson(), x.Total, x.Result)).ToList();
            byPercentage = false;
        }
        var passed = rows.Where(r => r.Result == "pass").OrderByDescending(r => r.Key).ToList();
        var merit = new int[passed.Count];
        for (var i = 0; i < passed.Count; i++)
        {
            merit[i] = i > 0 && passed[i - 1].Key == passed[i].Key ? merit[i - 1] : i + 1;
            passed[i].Row["merit"] = merit[i];
        }
        return new
        {
            title,
            className,
            basedOn = byPercentage ? "weighted percentage" : "total marks",
            rows = passed.Where((_, i) => merit[i] <= top).Select(r => r.Row),
        };
    }

    /* ---------- Revaluation ---------- */

    private static JsonObject Shape(RevaluationRequest r)
    {
        var o = new JsonObject
        {
            ["id"] = r.Id,
            ["examId"] = r.ExamId,
            ["studentId"] = r.StudentId,
            ["subjectId"] = r.SubjectId,
            ["originalMarks"] = r.OriginalMarks,
            ["newMarks"] = r.NewMarks,
            ["reason"] = r.Reason,
            ["status"] = r.Status,
            ["remarks"] = r.Remarks,
            ["decidedByName"] = r.DecidedByName,
            ["createdAt"] = JsonDefaults.Node(r.CreatedAt),
            ["examName"] = r.Exam?.Name,
            ["maxMarks"] = r.Exam?.MaxMarks,
            ["subjectName"] = r.Subject?.Name,
        };
        if (r.Student is { } s)
        {
            o["studentName"] = $"{s.FirstName} {s.LastName}";
            o["admissionNo"] = s.AdmissionNo;
        }
        return o;
    }

    public async Task<List<JsonObject>> ListRevaluationsAsync(string? status, int? studentId, int? examId, CancellationToken ct = default) =>
        (await db.Set<RevaluationRequest>().AsNoTracking().Include(r => r.Exam).Include(r => r.Subject).Include(r => r.Student)
            .Where(r => r.Exam != null && r.Subject != null && r.Student != null
                && (string.IsNullOrEmpty(status) || r.Status == status) && (studentId == null || r.StudentId == studentId) && (examId == null || r.ExamId == examId))
            .OrderByDescending(r => r.Id).Take(300).ToListAsync(ct))
        .Select(Shape).ToList();

    /// <summary>The student must have a mark in that exam and subject; one open request per student, exam and subject.</summary>
    public async Task<int> RequestRevaluationAsync(Body b, int requestedBy, CancellationToken ct = default)
    {
        if (b.IsBlankTrimmed("reason")) throw HttpException.BadRequest("A reason is required");
        int? examId = b.Int("examId"), studentId = b.Int("studentId"), subjectId = b.Int("subjectId");
        var mark = await db.Set<Mark>().AsNoTracking().FirstOrDefaultAsync(m => m.ExamId == examId && m.StudentId == studentId && m.SubjectId == subjectId, ct)
            ?? throw HttpException.NotFound("There is no recorded mark for that student, exam and subject");
        if (await db.Set<RevaluationRequest>().AnyAsync(r => r.ExamId == examId && r.StudentId == studentId && r.SubjectId == subjectId && r.Status == "pending", ct))
            throw HttpException.Conflict("A revaluation request is already pending for this subject");
        var row = new RevaluationRequest
        {
            ExamId = mark.ExamId, StudentId = mark.StudentId, SubjectId = mark.SubjectId, OriginalMarks = mark.Marks,
            Reason = b.Trimmed("reason"), RequestedBy = requestedBy,
        };
        db.Set<RevaluationRequest>().Add(row);
        await db.SaveChangesAsync(ct);
        return row.Id;
    }

    /// <summary>
    /// Accepting records the corrected mark on the exam itself in the same transaction, so results and report cards update at
    /// once; the request keeps the original mark as the audit trail. Marks may go up or down.
    /// </summary>
    public Task DecideRevaluationAsync(int id, string decision, Body b, CancellationToken ct = default) => db.InTransactionAsync(async () =>
    {
        var req = await db.Set<RevaluationRequest>().Include(r => r.Exam).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw HttpException.NotFound("Revaluation request not found");
        if (req.Status != "pending") throw HttpException.Conflict($"This request is already {req.Status}");
        var remarks = b.Str("remarks");
        if (decision == "accepted")
        {
            var max = req.Exam!.MaxMarks;
            if (!b.Has("newMarks") || (b.Raw("newMarks") is JsonValue v && v.TryGetValue<string>(out var s) && s == "")
                || b.Num("newMarks") is not { } marks || marks < 0 || marks > max)
                throw HttpException.BadRequest($"The revised mark must be between 0 and {max}");
            var mark = await db.Set<Mark>().FirstOrDefaultAsync(m => m.ExamId == req.ExamId && m.StudentId == req.StudentId && m.SubjectId == req.SubjectId, ct);
            if (mark is null)
            {
                mark = new Mark { ExamId = req.ExamId, StudentId = req.StudentId, SubjectId = req.SubjectId };
                db.Set<Mark>().Add(mark);
            }
            mark.Marks = (decimal)marks;
            req.Status = "accepted";
            req.NewMarks = (decimal)marks;
            req.Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(remarks)) throw HttpException.BadRequest("Give a reason for rejecting");
            req.Status = "rejected";
            req.Remarks = remarks.Trim();
        }
        req.DecidedBy = user.Id;
        req.DecidedByName = user.Name;
        await db.SaveChangesAsync(ct);
        return true;
    }, ct);
}
