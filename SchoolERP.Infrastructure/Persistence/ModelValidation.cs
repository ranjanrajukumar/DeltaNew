using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Domain.Common;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Persistence;

/// <summary>
/// The field rules the Node app's Sequelize models declared (validate: { min, max, is, isEmail }), checked on every save the
/// way Sequelize did: a new row checks every rule, a changed row only the fields that changed, and a null value is not checked.
/// A failure is a 400 with Sequelize's own message, e.g. "Validation min on capacity failed".
/// </summary>
internal static partial class ModelValidation
{
    private enum Kind { Min, Max, Is, IsEmail }

    private sealed record Rule(Type Entity, string Property, Kind Kind, decimal Limit = 0, Regex? Pattern = null);

    private static Rule Min<T>(string property, decimal limit) => new(typeof(T), property, Kind.Min, limit);
    private static Rule Max<T>(string property, decimal limit) => new(typeof(T), property, Kind.Max, limit);
    private static Rule Url<T>(string property) => new(typeof(T), property, Kind.Is, Pattern: HttpUrl());
    private static Rule Email<T>(string property) => new(typeof(T), property, Kind.IsEmail);

    private static IEnumerable<Rule> Range<T>(string property, decimal min, decimal max) => [Min<T>(property, min), Max<T>(property, max)];

    private static readonly ILookup<Type, Rule> Rules = new List<Rule>
    {
        Min<Activity>(nameof(Activity.Capacity), 1), Min<Activity>(nameof(Activity.Fee), 0),
        Min<Asset>(nameof(Asset.Cost), 0),
        Min<AssetMaintenance>(nameof(AssetMaintenance.Cost), 0),
        Min<Book>(nameof(Book.Copies), 0), Min<Book>(nameof(Book.Available), 0),
        Min<Exam>(nameof(Exam.MaxMarks), 1),
        Min<Expense>(nameof(Expense.Amount), 0.01m),
        Min<Fee>(nameof(Fee.Amount), 0.01m),
        Min<FeeAdjustment>(nameof(FeeAdjustment.Amount), 0.01m),
        Min<FeeGroupItem>(nameof(FeeGroupItem.Amount), 0.01m),
        Min<FeeStructure>(nameof(FeeStructure.Amount), 0),
        Min<Homework>(nameof(Homework.MaxMarks), 1),
        Min<HomeworkSubmission>(nameof(HomeworkSubmission.Marks), 0),
        Min<HostelAllocation>(nameof(HostelAllocation.SecurityDeposit), 0),
        Min<HostelRoom>(nameof(HostelRoom.Capacity), 1), Min<HostelRoom>(nameof(HostelRoom.MonthlyFee), 0),
        Min<Income>(nameof(Income.Amount), 0.01m),
        Min<InventoryItem>(nameof(InventoryItem.ReorderLevel), 0), Min<InventoryItem>(nameof(InventoryItem.Quantity), 0),
        Min<InventoryRequest>(nameof(InventoryRequest.Quantity), 1),
        Min<JobOpening>(nameof(JobOpening.Vacancies), 1),
        Min<Mark>(nameof(Mark.Marks), 0),
        Min<PurchaseOrderLine>(nameof(PurchaseOrderLine.Quantity), 1), Min<PurchaseOrderLine>(nameof(PurchaseOrderLine.UnitCost), 0),
        Min<Refund>(nameof(Refund.Amount), 0.01m),
        Min<Room>(nameof(Room.Capacity), 1),
        Min<StaffSalary>(nameof(StaffSalary.Basic), 0), Min<StaffSalary>(nameof(StaffSalary.Allowances), 0),
        Min<StaffSalary>(nameof(StaffSalary.Deductions), 0),
        Min<StockMovement>(nameof(StockMovement.Quantity), 1), Min<StockMovement>(nameof(StockMovement.UnitCost), 0),
        Min<TransportRoute>(nameof(TransportRoute.MonthlyFee), 0),
        Min<Vehicle>(nameof(Vehicle.Capacity), 1),
        Min<VehicleMaintenance>(nameof(VehicleMaintenance.Cost), 0),
        Url<LmsLesson>(nameof(LmsLesson.VideoUrl)), Url<LmsLesson>(nameof(LmsLesson.ResourceUrl)),
        Url<LmsLessonPart>(nameof(LmsLessonPart.Url)),
        Url<LmsMaterial>(nameof(LmsMaterial.Url)),
        Url<StudyMaterial>(nameof(StudyMaterial.Url)),
        new(typeof(TransportStop), nameof(TransportStop.PickupTime), Kind.Is, Pattern: ClockTime()),
        Email<Supplier>(nameof(Supplier.Email)),
        Email<User>(nameof(User.Email)),
    }
    .Concat(Range<Exam>(nameof(Exam.Weight), 1, 1000))
    .Concat(Range<GradeBand>(nameof(GradeBand.MinPercent), 0, 100))
    .Concat(Range<GradingSetting>(nameof(GradingSetting.PassPercent), 0, 100))
    .Concat(Range<LibraryMembership>(nameof(LibraryMembership.MaxBooks), 1, 50))
    .Concat(Range<LmsAssignment>(nameof(LmsAssignment.MaxScore), 1, 10000))
    .Concat(Range<PerformanceReview>(nameof(PerformanceReview.Work), 1, 5))
    .Concat(Range<PerformanceReview>(nameof(PerformanceReview.Punctuality), 1, 5))
    .Concat(Range<PerformanceReview>(nameof(PerformanceReview.Teamwork), 1, 5))
    .Concat(Range<PerformanceReview>(nameof(PerformanceReview.Initiative), 1, 5))
    .Concat(Range<PeriodAttendance>(nameof(PeriodAttendance.Period), 1, 12))
    .Concat(Range<RoomBooking>(nameof(RoomBooking.Period), 1, 12))
    .Concat(Range<StaffUnavailability>(nameof(StaffUnavailability.Day), 1, 7))
    .Concat(Range<StaffUnavailability>(nameof(StaffUnavailability.Period), 1, 12))
    .Concat(Range<StudentHealth>(nameof(StudentHealth.HeightCm), 30, 250))
    .Concat(Range<StudentHealth>(nameof(StudentHealth.WeightKg), 3, 250))
    .Concat(Range<TimetableSlot>(nameof(TimetableSlot.Day), 1, 7))
    .Concat(Range<TimetableSlot>(nameof(TimetableSlot.Period), 1, 12))
    .ToLookup(r => r.Entity);

    /// <summary>Throws a 400 for the first rule a new or changed row breaks.</summary>
    public static void Check(ChangeTracker tracker)
    {
        foreach (var entry in tracker.Entries<AuditableEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
            if (entry.State == EntityState.Added) CheckRequired(entry);
            foreach (var rule in Rules[entry.Entity.GetType()])
            {
                var property = entry.Property(rule.Property);
                if (entry.State == EntityState.Modified && !property.IsModified) continue;
                if (property.CurrentValue is not { } value) continue;
                if (!Passes(rule, value))
                    throw HttpException.BadRequest($"Validation {Name(rule.Kind)} on {Camel(rule.Property)} failed");
            }
        }
    }

    /// <summary>
    /// Sequelize's allowNull: false on a new row: "Subject.code cannot be null". Text left null and a date left unset
    /// (DateOnly's default) are the values a missing field ends up as here.
    /// </summary>
    private static void CheckRequired(EntityEntry<AuditableEntity> entry)
    {
        foreach (var property in entry.Properties)
        {
            var meta = property.Metadata;
            if (meta.IsNullable || meta.IsPrimaryKey() || meta.GetDefaultValueSql() is not null || meta.GetDefaultValue() is not null) continue;
            var missing = meta.ClrType == typeof(string) ? property.CurrentValue is null
                : meta.ClrType == typeof(DateOnly) && property.CurrentValue is DateOnly d && d == default;
            if (missing) throw HttpException.BadRequest($"{entry.Entity.GetType().Name}.{Camel(meta.Name)} cannot be null");
        }
    }

    private static bool Passes(Rule rule, object value) => rule.Kind switch
    {
        Kind.Min => Convert.ToDecimal(value, CultureInfo.InvariantCulture) >= rule.Limit,
        Kind.Max => Convert.ToDecimal(value, CultureInfo.InvariantCulture) <= rule.Limit,
        Kind.Is => rule.Pattern!.IsMatch(Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""),
        Kind.IsEmail => EmailPattern().IsMatch(Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""),
        _ => true,
    };

    private static string Name(Kind kind) => kind switch
    {
        Kind.Min => "min",
        Kind.Max => "max",
        Kind.Is => "is",
        _ => "isEmail",
    };

    private static string Camel(string name) => char.ToLowerInvariant(name[0]) + name[1..];

    [GeneratedRegex(@"^https?://\S+$", RegexOptions.IgnoreCase)]
    private static partial Regex HttpUrl();

    [GeneratedRegex("^([01][0-9]|2[0-3]):[0-5][0-9]$")]
    private static partial Regex ClockTime();

    /// <summary>The shape validator.js's isEmail accepts in practice: something@domain.tld, no spaces.</summary>
    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$")]
    private static partial Regex EmailPattern();
}
