using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Repositories;

/// <summary>Exam group lists and attendance summaries (Dapper).</summary>
public interface IExamQueryRepository
{
    public sealed record GroupRow(int Id, int ClassId, string Name, string ClassName);

    public sealed record GroupExamRow(int Id, string Name, int Weight, int GroupId);

    public sealed record AttendanceSummaryRow(int Days, int Attended);

    public sealed record StudentAttendanceRow(int StudentId, int Total, int Present);

    /// <summary>Groups whose class exists (newest first) and the exams in them (by id).</summary>
    Task<(List<GroupRow> Groups, List<GroupExamRow> Exams)> GroupsAsync(CancellationToken ct = default);

    /// <summary>How many attendance days a student has, and on how many they were not absent.</summary>
    Task<AttendanceSummaryRow> AttendanceSummaryAsync(int studentId, CancellationToken ct = default);

    /// <summary>Per active student of a class who has attendance: days marked and days not absent.</summary>
    Task<List<StudentAttendanceRow>> AttendanceByStudentAsync(int classId, CancellationToken ct = default);
}
