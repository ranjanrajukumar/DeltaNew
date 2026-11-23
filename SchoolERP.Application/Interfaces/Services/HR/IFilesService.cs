using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>Student and staff documents (the Node app's files/files.service.js and hr/staffFiles.js).</summary>
public interface IFilesService
{
    Task<List<JsonObject>> ListStudentFilesAsync(string? studentId, string? kind, CancellationToken ct = default);

    Task<JsonObject> SaveStudentFileAsync(string? studentIdText, string? kind, string? title, string? originalName, byte[]? bytes, CancellationToken ct = default);

    Task<FilesService.Download> ReadStudentFileAsync(int id, CancellationToken ct = default);

    Task DeleteStudentFileAsync(int id, CancellationToken ct = default);

    Task<List<JsonObject>> ListStaffFilesAsync(string? staffId, CancellationToken ct = default);

    Task<JsonObject> SaveStaffFileAsync(string? staffIdText, string? title, string? originalName, byte[]? bytes, CancellationToken ct = default);

    Task<FilesService.Download> ReadStaffFileAsync(int id, CancellationToken ct = default);

    Task DeleteStaffFileAsync(int id, CancellationToken ct = default);
}
