using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Links to study material per class and subject (the Node app's academics/materials.routes.js).</summary>
public sealed class StudyMaterialService(IStudyMaterialRepository materials, IUnitOfWork unitOfWork, ICurrentUser user) : IStudyMaterialService
{
    private static readonly string[] Fields = ["classId", "subjectId", "title", "url", "description"];

    public static JsonObject Shape(StudyMaterial m) => new()
    {
        ["id"] = m.Id,
        ["classId"] = m.ClassId,
        ["subjectId"] = m.SubjectId,
        ["title"] = m.Title,
        ["url"] = m.Url,
        ["description"] = m.Description,
        ["createdByName"] = m.CreatedByName,
        ["createdAt"] = JsonDefaults.Node(m.CreatedAt),
        ["className"] = m.SchoolClass is { } c ? $"{c.Name}-{c.Section}" : null,
        ["subjectName"] = m.Subject?.Name,
    };

    public async Task<List<JsonObject>> ListAsync(int? classId, int? subjectId, CancellationToken ct = default) =>
        (await materials.ListWithNamesAsync(classId, subjectId, ct)).Select(Shape).ToList();

    public async Task<int> CreateAsync(Body body, CancellationToken ct = default)
    {
        if (!body.Truthy("classId") || !body.Truthy("subjectId") || body.IsBlankTrimmed("title") || !body.Truthy("url"))
            throw HttpException.BadRequest("Class, subject, title and link are required");
        var m = new StudyMaterial { CreatedByName = user.Name };
        CrudService.Apply(body, Fields, m);
        materials.Add(m);
        await unitOfWork.SaveChangesAsync(ct);
        return m.Id;
    }

    public async Task UpdateAsync(int id, Body body, CancellationToken ct = default)
    {
        var m = await materials.FindAsync(id, ct) ?? throw HttpException.NotFound("Material not found");
        CrudService.Apply(body, Fields, m);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Teachers can remove what they added; an admin can remove anything.</summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var m = await materials.FindAsync(id, ct) ?? throw HttpException.NotFound("Material not found");
        if (user.Role != "admin" && m.CreatedByName != user.Name) throw HttpException.Conflict("You can only delete materials you added");
        materials.Remove(m);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
