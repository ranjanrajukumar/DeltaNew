using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Modules.Boards;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Which subjects each class studies (the Node app's academics/classSubjects.routes.js).</summary>
public sealed partial class ClassSubjectsService(
    IClassSubjectRepository classSubjects, IClassSubjectQueryRepository classSubjectQueries, ISubjectRepository subjects,
    IUnitOfWork unitOfWork, IBoardService boards, ISubjectMapService subjectMap) : IClassSubjectService
{
    /// <summary>Class–subject assignments with readable class, subject and teacher names, and how each subject is taught.</summary>
    public async Task<List<object>> ListAsync(CancellationToken ct = default)
    {
        var rows = await classSubjectQueries.ListAsync(ct); // inner joins on class and subject, as Sequelize's includes were
        return rows.Select(r => (object)new
        {
            id = r.Id,
            classId = r.ClassId,
            subjectId = r.SubjectId,
            staffId = r.StaffId,
            component = r.Component,
            componentLabel = SubjectMapService.ComponentLabel.GetValueOrDefault(r.Component),
            isElective = r.IsElective,
            electiveGroup = r.ElectiveGroup,
            periodsPerWeek = r.PeriodsPerWeek,
            practicalShare = r.PracticalShare,
            className = r.ClassName,
            subjectName = r.SubjectName,
            teacherName = r.TeacherName,
        }).ToList();
    }

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        var row = new ClassSubject();
        SubjectMapService.ApplyClassSubject(body, row);
        if (await classSubjects.ExistsAsync(row.ClassId, row.SubjectId, ct))
            throw HttpException.Conflict("That subject is already in this class");
        classSubjects.Add(row);
        await unitOfWork.SaveChangesAsync(ct);
        return row.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var row = await classSubjects.FindAsync(id, ct) ?? throw HttpException.NotFound("Not found");
        SubjectMapService.ApplyClassSubject(body, row, partial: true);
        await unitOfWork.SaveChangesAsync(ct);
        await subjectMap.AfterClassSubjectChangeAsync(row.Id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var row = await classSubjects.FindAsync(id, ct);
        if (row is null) return;
        classSubjects.Remove(row);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Gives each class (all, or the listed ones) its board's default subjects and periods per week. Subjects are created when
    /// missing; anything already assigned is kept, and only a missing periods-per-week is filled in.
    /// </summary>
    public async Task<object> ApplyTemplateAsync(JsonNode? classIds, CancellationToken ct = default)
    {
        var wanted = classIds is JsonArray list
            ? list.Select(n => Body.AsNumber(n) is { } d && d == Math.Floor(d) ? (int)d : 0).ToList()
            : [];
        var classes = await classSubjects.ClassesAsync(wanted, ct);
        if (classes.Count == 0) throw HttpException.BadRequest("No classes to apply a template to");

        var byName = (await subjects.ListTrackedAsync(ct))
            .GroupBy(s => s.Name.ToLowerInvariant()).ToDictionary(g => g.Key, g => g.Last());
        var codes = byName.Values.Select(s => s.Code).ToHashSet();
        var templates = new Dictionary<string, JsonArray?>(); // board|stage -> subjects
        var added = 0;
        var filled = 0;
        var noTemplate = new List<string>();

        foreach (var cls in classes)
        {
            var stage = SubjectTemplates.StageOf(cls.Name);
            var board = await boards.OfClassAsync(cls, ct);
            var key = $"{board?.Id}|{stage}";
            if (!templates.TryGetValue(key, out var templateSubjects))
            {
                var rule = stage is null ? null : await boards.ResolveRuleAsync(board, "subject_template", stage, ct: ct);
                templateSubjects = rule?.Payload?["subjects"] as JsonArray;
                templates[key] = templateSubjects;
            }
            if (templateSubjects is null)
            {
                noTemplate.Add($"{cls.Name}-{cls.Section}");
                continue;
            }
            foreach (var item in templateSubjects)
            {
                var name = Body.AsString(item?["name"]) ?? "";
                var periodsPerWeek = Body.AsNumber(item?["periodsPerWeek"]) is { } p ? (int?)p : null;
                var component = Body.AsString(item?["component"]) ?? "theory";
                if (!byName.TryGetValue(name.ToLowerInvariant(), out var subject))
                {
                    var letters = NotLetters().Replace(name, "");
                    var code = (letters.Length > 6 ? letters[..6] : letters).ToUpperInvariant();
                    if (code == "") code = "SUB";
                    for (var n = 2; codes.Contains(code); n++)
                    {
                        var next = $"{(code.Length > 6 ? code[..6] : code)}{n}";
                        code = next.Length > 20 ? next[..20] : next;
                    }
                    codes.Add(code);
                    subject = new Subject { Name = name, Code = code };
                    subjects.Add(subject);
                    await unitOfWork.SaveChangesAsync(ct);
                    byName[name.ToLowerInvariant()] = subject;
                }
                var link = await classSubjects.FindAsync(cls.Id, subject.Id, ct);
                if (link is null)
                {
                    classSubjects.Add(new ClassSubject
                    {
                        ClassId = cls.Id, SubjectId = subject.Id, Component = component, PeriodsPerWeek = periodsPerWeek,
                    });
                    await unitOfWork.SaveChangesAsync(ct);
                    added++;
                }
                else if (link.PeriodsPerWeek is null)
                {
                    link.PeriodsPerWeek = periodsPerWeek;
                    await unitOfWork.SaveChangesAsync(ct);
                    filled++;
                }
            }
        }
        return new { classes = classes.Count, added, filled, noTemplate };
    }

    [GeneratedRegex("[^A-Za-z]")]
    private static partial Regex NotLetters();
}
