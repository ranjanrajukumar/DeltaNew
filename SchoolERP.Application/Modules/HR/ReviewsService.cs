using System.Text.Json.Nodes;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Crud;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.HR;

/// <summary>Staff performance reviews (the Node app's hr/reviews.service.js). Admin only; a final review cannot change.</summary>
public sealed class ReviewsService(IPerformanceReviewRepository reviews, IStaffRepository staffRepo, IUnitOfWork unitOfWork) : IReviewsService
{
    private static readonly string[] Ratings = ["work", "punctuality", "teamwork", "initiative"];
    private static readonly string[] Fields = ["staffId", "period", "reviewDate", "reviewer", .. Ratings, "strengths", "improvements"];

    private static JsonObject Shape(PerformanceReview r)
    {
        var o = JsonDefaults.ToJsonObject(r);
        o.Remove("Staff");
        o["staffName"] = r.Staff is { } s ? $"{s.FirstName} {s.LastName}" : null;
        o["staffNo"] = r.Staff?.StaffNo;
        o["overall"] = Js.Round2((r.Work + r.Punctuality + r.Teamwork + r.Initiative) / 4.0);
        return o;
    }

    public async Task<List<JsonObject>> ListAsync(int? staffId, CancellationToken ct = default) =>
        (await reviews.ListWithStaffAsync(staffId, ct))
        .Select(Shape).ToList();

    private static void CheckRatings(Body b)
    {
        foreach (var k in Ratings)
            if (b.Has(k) && !(b.IsBlank(k) is false && b.Num(k) is { } n && n == Math.Floor(n) && n >= 1 && n <= 5))
                throw HttpException.BadRequest("Ratings must be whole numbers from 1 to 5");
    }

    public async Task<int> CreateAsync(Body b, CancellationToken ct = default)
    {
        CheckRatings(b);
        var staffId = b.Truthy("staffId") ? b.Int("staffId") : null;
        if (staffId is null || !await staffRepo.ExistsAsync(staffId.Value, ct)) throw HttpException.BadRequest("Choose a staff member");
        if (b.IsBlankTrimmed("period")) throw HttpException.BadRequest("Review period is required");
        if (!b.Truthy("reviewDate")) throw HttpException.BadRequest("Review date is required");
        if (Ratings.Any(k => !b.Has(k))) throw HttpException.BadRequest("Every rating is required");
        var review = new PerformanceReview();
        CrudService.Apply(b, Fields, review);
        reviews.Add(review);
        await unitOfWork.SaveChangesAsync(ct);
        return review.Id;
    }

    public async Task UpdateAsync(int id, Body b, CancellationToken ct = default)
    {
        var review = await reviews.FindAsync(id, ct) ?? throw HttpException.NotFound("Review not found");
        if (review.Status == "final") throw HttpException.Conflict("A finalised review cannot be changed");
        CheckRatings(b);
        CrudService.Apply(b, Fields.Where(f => f != "staffId").ToArray(), review); // the reviewed person is fixed once the review exists
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task FinalizeAsync(int id, CancellationToken ct = default)
    {
        var review = await reviews.FindAsync(id, ct) ?? throw HttpException.NotFound("Review not found");
        if (review.Status == "final") throw HttpException.Conflict("This review is already final");
        review.Status = "final";
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var review = await reviews.FindAsync(id, ct) ?? throw HttpException.NotFound("Review not found");
        if (review.Status == "final") throw HttpException.Conflict("A finalised review cannot be deleted");
        reviews.Remove(review);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
