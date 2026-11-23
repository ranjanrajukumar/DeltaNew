using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;
using SchoolERP.Contracts.Academics;
using System.Text.Json.Nodes;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>Academic sessions (the Node app's academics/academicYears.routes.js). Exactly one is current.</summary>
public sealed class AcademicYearService(IAcademicYearRepository years, IUnitOfWork unitOfWork) : IAcademicYearService
{
    private static readonly string[] Fields = ["name", "startDate", "endDate"];

    private static void Check(DateOnly start, DateOnly end)
    {
        if (start != default && end != default && end <= start) throw HttpException.BadRequest("End date must be after the start date");
    }

    public async Task<List<AcademicYearResponse>> ListAsync(CancellationToken ct = default) =>
        (await years.ListByStartDescendingAsync(ct)).Select(year => new AcademicYearResponse(
            year.Id, year.Name, year.StartDate, year.EndDate, year.IsCurrent)).ToList();

    public async Task<int> CreateAsync(AcademicYearRequest request, CancellationToken ct = default)
    {
        var year = new AcademicYear();
        CrudService.Apply(ToBody(request), Fields, year, blankAsNull: false);
        Check(year.StartDate, year.EndDate);
        years.Add(year);
        await unitOfWork.SaveChangesAsync(ct);
        return year.Id;
    }

    public async Task UpdateAsync(int id, AcademicYearRequest request, CancellationToken ct = default)
    {
        var year = await years.FindAsync(id, ct) ?? throw HttpException.NotFound("Academic year not found");
        CrudService.Apply(ToBody(request), Fields, year, blankAsNull: false);
        Check(year.StartDate, year.EndDate);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Makes this year the current one and clears the flag on every other year.</summary>
    public Task SetCurrentAsync(int id, CancellationToken ct = default) => unitOfWork.InTransactionAsync(async () =>
    {
        var year = await years.FindAsync(id, ct) ?? throw HttpException.NotFound("Academic year not found");
        foreach (var other in await years.OtherCurrentYearsAsync(id, ct)) other.IsCurrent = false;
        year.IsCurrent = true;
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }, ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var year = await years.FindAsync(id, ct) ?? throw HttpException.NotFound("Academic year not found");
        years.Remove(year);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static Body ToBody(AcademicYearRequest request)
    {
        var json = new JsonObject();
        if (request.Name is not null) json["name"] = request.Name;
        if (request.StartDate is { } start) json["startDate"] = start.ToString("yyyy-MM-dd");
        if (request.EndDate is { } end) json["endDate"] = end.ToString("yyyy-MM-dd");
        return new Body(json);
    }
}
