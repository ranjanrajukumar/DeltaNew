using System.Globalization;
using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Exams;

/// <summary>Exams, mark entry, results and report cards (the Node app's exams/exams.service.js).</summary>
public sealed class ExamsService(
    IExamRepository examRepo, IMarkRepository markRepo, IExamQueryRepository examQueries, IUnitOfWork unitOfWork,
    ISubjectMapService subjectMap, IGradingService grading, IBoardService boards) : IExamsService
{
    public static readonly string[] Fields = ["name", "classId", "startDate", "endDate", "maxMarks"];

    public sealed record ExamSubject(int Id, string Name, string Code, string Component, bool IsElective, int? PracticalShare);

    /// <summary>One student's result in an exam.</summary>
    public sealed record ResultRow(int StudentId, string AdmissionNo, string Name, double Total, int OutOf, double Percentage, string Grade, string Result)
    {
        public int Rank { get; set; }

        public JsonObject ToJson() => new()
        {
            ["studentId"] = StudentId, ["admissionNo"] = AdmissionNo, ["name"] = Name, ["total"] = Total, ["outOf"] = OutOf,
            ["percentage"] = Percentage, ["grade"] = Grade, ["result"] = Result, ["rank"] = Rank,
        };
    }

    public sealed record Results(Exam Exam, List<ResultRow> Rows);

    public async Task<List<JsonObject>> ListAsync(CancellationToken ct = default)
    {
        var rows = await examRepo.ListWithClassAsync(ct);
        return rows.Select(e =>
        {
            var o = JsonDefaults.ToJsonObject(e);
            o.Remove("SchoolClass");
            o["className"] = $"{e.SchoolClass!.Name}-{e.SchoolClass.Section}";
            return o;
        }).ToList();
    }

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        if (body.IsBlankTrimmed("name") || !body.Truthy("classId")) throw HttpException.BadRequest("Name and class are required");
        var exam = new Exam();
        CrudService.Apply(body, Fields, exam);
        examRepo.Add(exam);
        await unitOfWork.SaveChangesAsync(ct);
        return exam.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var exam = await examRepo.FindAsync(id, ct) ?? throw HttpException.NotFound("Exam not found");
        CrudService.Apply(body, Fields, exam);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var exam = await examRepo.FindAsync(id, ct) ?? throw HttpException.NotFound("Exam not found");
        examRepo.Remove(exam);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<Exam> LoadAsync(int id, CancellationToken ct = default) =>
        await examRepo.GetAsync(id, ct) ?? throw HttpException.NotFound("Exam not found");

    /// <summary>The subjects taught in the exam's class, in a stable order.</summary>
    public async Task<List<ExamSubject>> SubjectsAsync(Exam exam, CancellationToken ct = default) =>
        (await examRepo.ClassSubjectsAsync(exam.ClassId, ct))
        .Select(r => new ExamSubject(r.Subject!.Id, r.Subject.Name, r.Subject.Code, r.Component, r.IsElective, r.PracticalShare)).ToList();

    /// <summary>
    /// Which of the exam's subjects this user may enter marks for: all for an admin (null); for a teacher only the subjects
    /// assigned to them in Class subjects, through the staff record with the same email as their login.
    /// </summary>
    private async Task<HashSet<int>?> EditableSubjectIdsAsync(int userId, string? role, Exam exam, CancellationToken ct)
    {
        if (role == "admin") return null;
        var email = await examRepo.UserEmailAsync(userId, ct) ?? "";
        var staff = await examRepo.ActiveStaffIdsByEmailAsync(email, ct);
        if (staff.Count == 0) return [];
        return (await examRepo.SubjectIdsTaughtAsync(exam.ClassId, staff, ct)).ToHashSet();
    }

    private static JsonNode? SplitJson((int Theory, int Practical)? split) =>
        split is { } s ? new JsonObject { ["theory"] = s.Theory, ["practical"] = s.Practical } : null;

    private static string Key(int id) => id.ToString(CultureInfo.InvariantCulture);

    /// <summary>The mark-entry grid: every active student of the class against every subject of the class.</summary>
    public async Task<object> MarkSheetAsync(int examId, int userId, string? role, CancellationToken ct = default)
    {
        var exam = await LoadAsync(examId, ct);
        var facts = await subjectMap.ElectiveFactsAsync(exam.ClassId, ct);
        var subjects = await SubjectsAsync(exam, ct);
        var students = await examRepo.ActiveStudentsAsync(exam.ClassId, ct);
        var marks = await markRepo.OfExamAsync(examId, ct);
        var editable = await EditableSubjectIdsAsync(userId, role, exam, ct);

        var byStudent = new Dictionary<int, JsonObject>();
        var parts = new Dictionary<int, JsonObject>();
        foreach (var m in marks)
        {
            if (!byStudent.TryGetValue(m.StudentId, out var o)) byStudent[m.StudentId] = o = [];
            o[Key(m.SubjectId)] = m.Marks;
            if (m.PracticalMarks is { } p)
            {
                if (!parts.TryGetValue(m.StudentId, out var po)) parts[m.StudentId] = po = [];
                po[Key(m.SubjectId)] = new JsonObject { ["theory"] = m.Marks - p, ["practical"] = p };
            }
        }
        return new
        {
            exam,
            subjects = subjects.Select(s => new
            {
                id = s.Id, name = s.Name, code = s.Code, component = s.Component, elective = s.IsElective,
                split = SplitJson(SubjectMapService.SplitOf(s.Component, s.PracticalShare, exam.MaxMarks)),
                editable = editable is null || editable.Contains(s.Id),
            }),
            students = students.Select(s => new
            {
                studentId = s.Id,
                admissionNo = s.AdmissionNo,
                name = $"{s.FirstName} {s.LastName}",
                marks = byStudent.GetValueOrDefault(s.Id) ?? [],
                parts = parts.GetValueOrDefault(s.Id) ?? [], // theory and practical, for subjects marked in two parts
                // electives this student did not choose: no mark is expected or accepted
                skips = subjects.Where(sub => !facts.Takes(s.Id, sub.Id)).Select(sub => sub.Id),
            }),
        };
    }

    /// <summary>
    /// Saves marks atomically. A blank mark clears the entry; anything else must be a number between 0 and the exam's
    /// maximum (or each part's maximum for theory + practical). The whole batch is rejected if any row is invalid.
    /// </summary>
    public async Task<int> SaveMarksAsync(int examId, JsonNode? recordsNode, int userId, string? role, CancellationToken ct = default)
    {
        var exam = await LoadAsync(examId, ct);
        var records = recordsNode is null ? [] : recordsNode as JsonArray ?? throw HttpException.BadRequest("records must be a list");
        var rows = records.Select(n => new Body(n as JsonObject)).ToList();
        var editable = await EditableSubjectIdsAsync(userId, role, exam, ct);
        if (editable is not null && rows.Any(r => r.Num("subjectId") is not { } sid || !editable.Contains((int)sid)))
            throw HttpException.Forbidden(editable.Count > 0
                ? "You can only enter marks for the subjects assigned to you in this class"
                : "You have no subjects assigned in this class. An admin must assign you in Class subjects, and your login email must match your staff profile email.");

        var inClass = (await examRepo.StudentIdsInClassAsync(exam.ClassId, ct)).ToHashSet();
        var subjects = await SubjectsAsync(exam, ct);
        var subjectById = subjects.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());
        var facts = await subjectMap.ElectiveFactsAsync(exam.ClassId, ct);
        var clean = new List<(int StudentId, int SubjectId, decimal? Marks, decimal? Practical)>();
        foreach (var r in rows)
        {
            if (!r.Truthy("studentId") || !r.Truthy("subjectId")) throw HttpException.BadRequest("Each mark needs a student and a subject");
            var studentId = r.Num("studentId") is { } sn && sn == Math.Floor(sn) ? (int)sn : -1;
            var subjectId = r.Num("subjectId") is { } bn && bn == Math.Floor(bn) ? (int)bn : -1;
            if (!inClass.Contains(studentId)) throw HttpException.BadRequest("Every student must belong to the exam class");
            if (!subjectById.TryGetValue(subjectId, out var sub)) throw HttpException.BadRequest("Every subject must be one the exam class studies");
            var split = SubjectMapService.SplitOf(sub.Component, sub.PracticalShare, exam.MaxMarks);
            var twoParts = r.Has("theory") || r.Has("practical");
            if (twoParts && split is null)
                throw HttpException.BadRequest($"{sub.Name} is marked as one total: send marks, not theory and practical");
            decimal? total;
            decimal? practicalMarks = null;
            if (twoParts)
            {
                // theory + practical: each part is checked against its own maximum and the subject total is their sum
                foreach (var (name, key, max) in new[] { ("theory", "theory", split!.Value.Theory), ("practical", "practical", split.Value.Practical) })
                {
                    if (r.IsBlank(key)) continue;
                    if (max == 0) throw HttpException.BadRequest($"{sub.Name} has no {name} marks");
                    if (r.Num(key) is not { } v || v < 0 || v > max) throw HttpException.BadRequest($"{sub.Name}: {name} marks must be between 0 and {max}");
                }
                var both = !r.IsBlank("theory") && !r.IsBlank("practical");
                var only = split!.Value.Theory == 0 ? !r.IsBlank("practical") : split.Value.Practical == 0 && !r.IsBlank("theory");
                if (r.IsBlank("theory") && r.IsBlank("practical")) total = null;
                else if (both || only)
                {
                    var theory = (decimal)(r.Num("theory") ?? 0);
                    var practical = (decimal)(r.Num("practical") ?? 0);
                    total = theory + practical;
                    practicalMarks = practical;
                }
                else throw HttpException.BadRequest($"{sub.Name}: enter both the theory and the practical mark, or leave both empty");
            }
            else
            {
                var blank = r.IsBlank("marks");
                var value = r.Num("marks");
                if (!blank && (value is not { } v || v < 0 || v > exam.MaxMarks)) throw HttpException.BadRequest($"Marks must be between 0 and {exam.MaxMarks}");
                total = blank ? null : (decimal)value!.Value;
            }
            if (total is not null && !facts.Takes(studentId, subjectId))
                throw HttpException.BadRequest($"This student did not choose the elective {sub.Name}, so no mark can be entered for it");
            clean.Add((studentId, subjectId, total, practicalMarks));
        }

        await unitOfWork.InTransactionAsync(async () =>
        {
            foreach (var (studentId, subjectId, marks, practical) in clean)
            {
                var row = await markRepo.FindAsync(examId, studentId, subjectId, ct);
                if (marks is null)
                {
                    if (row is not null) markRepo.Remove(row);
                }
                else
                {
                    if (row is null)
                    {
                        row = new Mark { ExamId = examId, StudentId = studentId, SubjectId = subjectId };
                        markRepo.Add(row);
                    }
                    row.Marks = marks.Value;
                    row.PracticalMarks = practical;
                }
                await unitOfWork.SaveChangesAsync(ct);
            }
            return true;
        }, ct);
        return clean.Count;
    }

    /// <summary>Totals, percentage, grade and rank for every student who has marks in the exam. Equal totals share a rank (1, 1, 3 ...).</summary>
    public async Task<Results> ResultsAsync(int examId, GradingPolicy? policy = null, CancellationToken ct = default)
    {
        var exam = await LoadAsync(examId, ct);
        policy ??= await grading.ForClassAsync(exam.ClassId, ct);
        var subjects = await SubjectsAsync(exam, ct);
        var facts = await subjectMap.ElectiveFactsAsync(exam.ClassId, ct);
        var students = await examRepo.StudentsInClassAsync(exam.ClassId, ct);
        var ids = students.Select(s => s.Id).ToList();
        var marks = (await markRepo.OfExamAsync(examId, ids, ct))
            .GroupBy(m => m.StudentId).ToDictionary(g => g.Key, g => g.ToList());

        var rows = students.Where(s => marks.ContainsKey(s.Id)).Select(s =>
        {
            var taken = subjects.Where(sub => facts.Takes(s.Id, sub.Id)).ToList();
            var takenIds = taken.Select(sub => sub.Id).ToHashSet();
            var own = marks[s.Id].Where(m => takenIds.Contains(m.SubjectId)).ToList(); // a stale mark for a subject not taken is ignored
            var total = own.Sum(m => (double)m.Marks);
            var outOf = taken.Count * exam.MaxMarks;
            var percentage = outOf > 0 ? Js.Round2(total / outOf * 100) : 0;
            var failed = own.Count(m => (double)m.Marks / exam.MaxMarks * 100 < policy.PassPercent);
            var incomplete = own.Count < taken.Count; // fewer entered marks than subjects: not finished
            return new ResultRow(s.Id, s.AdmissionNo, $"{s.FirstName} {s.LastName}", Js.Round2(total), outOf, percentage,
                policy.GradeFor(percentage), incomplete ? "incomplete" : failed > 0 ? "fail" : "pass");
        }).OrderByDescending(r => r.Total).ToList();
        for (var i = 0; i < rows.Count; i++) rows[i].Rank = i > 0 && rows[i - 1].Total == rows[i].Total ? rows[i - 1].Rank : i + 1;
        return new Results(exam, rows);
    }

    public async Task<object> ResultsJsonAsync(int examId, CancellationToken ct = default)
    {
        var r = await ResultsAsync(examId, ct: ct);
        return new { exam = r.Exam, rows = r.Rows.Select(x => x.ToJson()) };
    }

    /// <summary>One student's report card: marks per subject, overall result, rank and attendance.</summary>
    public async Task<JsonObject> ReportCardAsync(int examId, int studentId, CancellationToken ct = default)
    {
        var policy = await grading.ForClassAsync((await LoadAsync(examId, ct)).ClassId, ct);
        var (exam, rows) = await ResultsAsync(examId, policy, ct);
        var student = await examRepo.StudentWithClassAsync(studentId, ct)
            ?? throw HttpException.NotFound("Student not found");
        var teacher = student.SchoolClass?.ClassTeacherId is { } tid
            ? await examRepo.StaffNameAsync(tid, ct)
            : null;

        var facts = await subjectMap.ElectiveFactsAsync(exam.ClassId, ct);
        var subjects = (await SubjectsAsync(exam, ct)).Where(s => facts.Takes(studentId, s.Id)).ToList();
        var marks = await markRepo.OfStudentAsync(examId, studentId, ct);
        var marksBySubject = marks.GroupBy(m => m.SubjectId).ToDictionary(g => g.Key, g => (double)g.Last().Marks);
        var practicalBySubject = marks.Where(m => m.PracticalMarks is not null).GroupBy(m => m.SubjectId).ToDictionary(g => g.Key, g => (double)g.Last().PracticalMarks!.Value);

        var attendance = await examQueries.AttendanceSummaryAsync(studentId, ct);

        var board = await boards.OfClassAsync(student.SchoolClass, ct) ?? throw HttpException.Conflict("Add a board first");
        var layout = (await boards.ResolveRuleAsync(board, "report_card", ct: ct))?.Payload;
        var showAffiliation = !(layout?["showAffiliation"] is JsonValue sa && sa.TryGetValue<bool>(out var show) && !show);
        var affiliation = showAffiliation ? await boards.AffiliationStatementAsync(board, student.SchoolClass?.BranchId, ct) : null;

        var subjectRows = new JsonArray();
        foreach (var s in subjects)
        {
            double? obtained = marksBySubject.TryGetValue(s.Id, out var m) ? m : null;
            double? percent = obtained is { } o ? o / exam.MaxMarks * 100 : null;
            var row = new JsonObject { ["subjectId"] = s.Id, ["subject"] = s.Name, ["component"] = s.Component, ["elective"] = s.IsElective };
            var split = SubjectMapService.SplitOf(s.Component, s.PracticalShare, exam.MaxMarks);
            if (split is { } sp && practicalBySubject.TryGetValue(s.Id, out var practical) && marksBySubject.ContainsKey(s.Id))
            {
                row["theoryMarks"] = marksBySubject[s.Id] - practical;
                row["practicalMarks"] = practical;
                row["theoryMax"] = sp.Theory;
                row["practicalMax"] = sp.Practical;
            }
            else if (split is { } sp2)
            {
                row["theoryMax"] = sp2.Theory;
                row["practicalMax"] = sp2.Practical;
            }
            row["marks"] = obtained;
            row["maxMarks"] = exam.MaxMarks;
            row["grade"] = percent is { } pc ? policy.GradeFor(pc) : null;
            row["passed"] = percent is { } pp ? pp >= policy.PassPercent : null;
            subjectRows.Add(row);
        }
        return new JsonObject
        {
            ["board"] = new JsonObject { ["name"] = board.Name, ["affiliation"] = affiliation },
            ["layout"] = layout?.DeepClone(),
            ["exam"] = new JsonObject { ["id"] = exam.Id, ["name"] = exam.Name, ["maxMarks"] = exam.MaxMarks },
            ["student"] = new JsonObject
            {
                ["id"] = student.Id,
                ["admissionNo"] = student.AdmissionNo,
                ["name"] = $"{student.FirstName} {student.LastName}",
                ["className"] = student.SchoolClass is { } c ? $"{c.Name}-{c.Section}" : null,
                ["classTeacher"] = teacher,
            },
            ["subjects"] = subjectRows,
            ["summary"] = rows.FirstOrDefault(r => r.StudentId == studentId)?.ToJson(),
            ["classSize"] = rows.Count,
            ["attendance"] = new JsonObject
            {
                ["days"] = attendance.Days,
                ["percentage"] = attendance.Days > 0 ? Js.Round2(attendance.Attended * 100.0 / attendance.Days) : null,
            },
        };
    }
}
