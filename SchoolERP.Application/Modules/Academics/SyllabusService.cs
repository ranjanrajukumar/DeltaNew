using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Syllabus topics and how far each class has got (the Node app's academics/syllabus.routes.js).</summary>
public sealed class SyllabusService(ISyllabusRepository syllabus, ISyllabusQueryRepository syllabusQueries, IUnitOfWork unitOfWork) : ISyllabusService
{
    public static readonly string[] Statuses = ["not_started", "in_progress", "completed"];
    private static readonly string[] Fields = ["classId", "subjectId", "sequence", "title", "objective", "plannedDate", "status"];

    /// <summary>Topics in teaching order.</summary>
    public Task<List<SyllabusTopic>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default) => syllabus.ListAsync(classId, subjectId, ct);

    /// <summary>The share of topics completed for each class and subject, and how many planned dates have slipped.</summary>
    public async Task<List<object>> ProgressAsync(CancellationToken ct = default) =>
        (await syllabusQueries.ProgressAsync(Clock.Today(), ct))
            .OrderBy(g => g.ClassName, StringComparer.InvariantCulture).ThenBy(g => g.SubjectName, StringComparer.InvariantCulture)
            .Select(g => (object)new
            {
                classId = g.ClassId,
                subjectId = g.SubjectId,
                className = g.ClassName,
                subjectName = g.SubjectName,
                total = g.Total,
                completed = g.Completed,
                inProgress = g.InProgress,
                // planned for a date that has passed but not finished: the plan is slipping
                behind = g.Behind,
                percent = (int)Js.Round(g.Completed * 100.0 / g.Total),
            })
            .ToList();

    private static void CheckStatus(Body body)
    {
        if (body.Truthy("status") && !Statuses.Contains(body.Str("status"))) throw HttpException.BadRequest("Status is invalid");
    }

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        if (!body.Truthy("classId") || !body.Truthy("subjectId") || body.IsBlankTrimmed("title"))
            throw HttpException.BadRequest("Class, subject and title are required");
        CheckStatus(body);
        var topic = new SyllabusTopic();
        CrudService.Apply(body, Fields, topic);
        syllabus.Add(topic);
        await unitOfWork.SaveChangesAsync(ct);
        return topic.Id;
    }

    /// <summary>Completing a topic stamps the date; moving it back clears the stamp.</summary>
    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var topic = await syllabus.FindAsync(id, ct) ?? throw HttpException.NotFound("Topic not found");
        CheckStatus(body);
        var wasCompleted = topic.Status == "completed";
        CrudService.Apply(body, Fields, topic);
        if (body.Truthy("status"))
        {
            if (topic.Status == "completed") { if (!wasCompleted) topic.CompletedOn = Clock.Today(); }
            else topic.CompletedOn = null;
        }
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var topic = await syllabus.FindAsync(id, ct) ?? throw HttpException.NotFound("Topic not found");
        syllabus.Remove(topic);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
