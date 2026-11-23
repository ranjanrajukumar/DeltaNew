using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Academics;

/// <summary>A class row as the list returns it: the class's columns plus studentCount and classTeacherName.</summary>
public sealed record ClassListRow(
    int Id, string Name, int? BranchId, int? ClassTeacherId, int? BoardId, string Board, string? Room, string Section,
    string? Authadd, string? Authedit, string? Authdel, DateTime? Addondt, DateTime? Editondt, DateTime? Delondt, bool Delstatus,
    int StudentCount, string? ClassTeacherName);
