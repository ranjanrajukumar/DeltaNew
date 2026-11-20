using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Files;

/// <summary>
/// Student and staff documents (the Node app's files/files.service.js and hr/staffFiles.js). The bytes live in the database row
/// (base64); rows saved before that move are read from the upload folder. The type is confirmed from the file's own first bytes.
/// </summary>
public sealed class FilesService(IApplicationDbContext db, IFileStorage storage, ICurrentUser user)
{
    public const int MaxBytes = 5 * 1024 * 1024;

    private static readonly (string Type, Func<byte[], bool> Matches)[] Signatures =
    [
        ("image/jpeg", b => b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF),
        ("image/png", b => b.Length >= 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })),
        ("image/webp", b => b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F' && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P'),
        ("application/pdf", b => b.Length >= 5 && b[0] == '%' && b[1] == 'P' && b[2] == 'D' && b[3] == 'F' && b[4] == '-'),
    ];

    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        ["photo"] = ["image/jpeg", "image/png", "image/webp"],
        ["document"] = ["image/jpeg", "image/png", "image/webp", "application/pdf"],
    };

    /// <summary>The file's real type from its first bytes, or null when it is none of the allowed kinds.</summary>
    public static string? Sniff(byte[] bytes) => Signatures.FirstOrDefault(s => s.Matches(bytes)).Type;

    private static string NewStoredName() => Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();

    private static string Cut(string text, int max) => text.Length > max ? text[..max] : text;

    private static void CheckSize(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0) throw HttpException.BadRequest("No file received");
        if (bytes.Length > MaxBytes) throw HttpException.BadRequest($"File is larger than {MaxBytes / 1024 / 1024} MB");
    }

    /// <summary>The bytes: from the row, or from the upload folder for rows saved before files moved into the database.</summary>
    private async Task<byte[]?> BytesAsync(string? data, string storedName, CancellationToken ct) =>
        data is not null ? Convert.FromBase64String(data) : await storage.ReadAsync(storedName, ct);

    public sealed record Download(string MimeType, string OriginalName, byte[] Bytes);

    /* ---------- Student files ---------- */

    private static JsonObject Shape(StudentFile f) => new()
    {
        ["id"] = f.Id, ["studentId"] = f.StudentId, ["kind"] = f.Kind, ["title"] = f.Title, ["originalName"] = f.OriginalName,
        ["mimeType"] = f.MimeType, ["size"] = f.Size, ["uploadedByName"] = f.UploadedByName, ["createdAt"] = JsonDefaults.Node(f.CreatedAt),
    };

    public async Task<List<JsonObject>> ListStudentFilesAsync(string? studentId, string? kind, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(studentId)) throw HttpException.BadRequest("studentId is required");
        var id = int.TryParse(studentId, out var n) ? n : -1;
        return (await db.Set<StudentFile>().AsNoTracking().Where(f => f.StudentId == id && (string.IsNullOrEmpty(kind) || f.Kind == kind))
            .OrderByDescending(f => f.Id).Select(f => new StudentFile
            {
                Id = f.Id, StudentId = f.StudentId, Kind = f.Kind, Title = f.Title, OriginalName = f.OriginalName, MimeType = f.MimeType,
                Size = f.Size, StoredName = f.StoredName, UploadedByName = f.UploadedByName, CreatedAt = f.CreatedAt,
            }).ToListAsync(ct)).Select(Shape).ToList();
    }

    /// <summary>A student has one photo: uploading a new one replaces the old one. Documents accumulate.</summary>
    public async Task<JsonObject> SaveStudentFileAsync(string? studentIdText, string? kind, string? title, string? originalName, byte[]? bytes, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(studentIdText)) throw HttpException.BadRequest("studentId is required");
        if (kind is null || !Allowed.ContainsKey(kind)) throw HttpException.BadRequest("kind must be photo or document");
        CheckSize(bytes);
        var studentId = int.TryParse(studentIdText, out var n) ? n : -1;
        if (!await db.Set<Student>().AnyAsync(s => s.Id == studentId, ct)) throw HttpException.NotFound("Student not found");
        var mime = Sniff(bytes!);
        if (mime is null || !Allowed[kind].Contains(mime))
            throw HttpException.BadRequest(kind == "photo" ? "A photo must be a JPEG, PNG or WebP image" : "Only PDF, JPEG, PNG or WebP files are allowed");
        var name = Cut(string.IsNullOrEmpty(originalName) ? "file" : originalName, 255);
        var label = (title ?? "").Trim() is { Length: > 0 } t ? t : kind == "photo" ? "Photo" : name;

        var replaced = kind == "photo" ? await db.Set<StudentFile>().Where(f => f.StudentId == studentId && f.Kind == "photo").ToListAsync(ct) : [];
        var row = new StudentFile
        {
            StudentId = studentId, Kind = kind, Title = Cut(label, 160), OriginalName = name, MimeType = mime, Size = bytes!.Length,
            StoredName = NewStoredName(), Data = Convert.ToBase64String(bytes), UploadedByName = user.Name,
        };
        db.Set<StudentFile>().Add(row);
        await db.SaveChangesAsync(ct);
        if (replaced.Count > 0)
        {
            db.Set<StudentFile>().RemoveRange(replaced);
            await db.SaveChangesAsync(ct);
            await storage.RemoveAsync(replaced.Select(r => r.StoredName), ct);
        }
        return Shape(row);
    }

    public async Task<Download> ReadStudentFileAsync(int id, CancellationToken ct = default)
    {
        var row = await db.Set<StudentFile>().AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct) ?? throw HttpException.NotFound("File not found");
        var bytes = await BytesAsync(row.Data, row.StoredName, ct) ?? throw HttpException.NotFound("File is missing from storage");
        return new Download(row.MimeType, row.OriginalName, bytes);
    }

    public async Task DeleteStudentFileAsync(int id, CancellationToken ct = default)
    {
        var row = await db.Set<StudentFile>().FirstOrDefaultAsync(f => f.Id == id, ct) ?? throw HttpException.NotFound("File not found");
        db.Set<StudentFile>().Remove(row);
        await db.SaveChangesAsync(ct);
        await storage.RemoveAsync([row.StoredName], ct);
    }

    /* ---------- Staff files ---------- */

    private static JsonObject Shape(StaffFile f) => new()
    {
        ["id"] = f.Id, ["staffId"] = f.StaffId, ["title"] = f.Title, ["originalName"] = f.OriginalName, ["mimeType"] = f.MimeType,
        ["size"] = f.Size, ["uploadedByName"] = f.UploadedByName, ["createdAt"] = JsonDefaults.Node(f.CreatedAt),
    };

    public async Task<List<JsonObject>> ListStaffFilesAsync(string? staffId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(staffId)) throw HttpException.BadRequest("staffId is required");
        var id = int.TryParse(staffId, out var n) ? n : -1;
        return (await db.Set<StaffFile>().AsNoTracking().Where(f => f.StaffId == id).OrderByDescending(f => f.Id)
            .Select(f => new StaffFile
            {
                Id = f.Id, StaffId = f.StaffId, Title = f.Title, OriginalName = f.OriginalName, MimeType = f.MimeType, Size = f.Size,
                StoredName = f.StoredName, UploadedByName = f.UploadedByName, CreatedAt = f.CreatedAt,
            }).ToListAsync(ct)).Select(Shape).ToList();
    }

    /// <summary>Contracts, certificates and ID proofs: the same content checks and storage as student documents.</summary>
    public async Task<JsonObject> SaveStaffFileAsync(string? staffIdText, string? title, string? originalName, byte[]? bytes, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(staffIdText)) throw HttpException.BadRequest("staffId is required");
        CheckSize(bytes);
        var staffId = int.TryParse(staffIdText, out var n) ? n : -1;
        if (!await db.Set<Staff>().AnyAsync(s => s.Id == staffId, ct)) throw HttpException.NotFound("Staff member not found");
        var mime = Sniff(bytes!) ?? throw HttpException.BadRequest("Only PDF, JPEG, PNG or WebP files are allowed");
        var name = Cut(string.IsNullOrEmpty(originalName) ? "file" : originalName, 255);
        var row = new StaffFile
        {
            StaffId = staffId, Title = Cut((title ?? "").Trim() is { Length: > 0 } t ? t : name, 160), OriginalName = name, MimeType = mime,
            Size = bytes!.Length, StoredName = NewStoredName(), Data = Convert.ToBase64String(bytes), UploadedByName = user.Name,
        };
        db.Set<StaffFile>().Add(row);
        await db.SaveChangesAsync(ct);
        return Shape(row);
    }

    public async Task<Download> ReadStaffFileAsync(int id, CancellationToken ct = default)
    {
        var row = await db.Set<StaffFile>().AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct) ?? throw HttpException.NotFound("File not found");
        var bytes = await BytesAsync(row.Data, row.StoredName, ct) ?? throw HttpException.NotFound("File is missing from storage");
        return new Download(row.MimeType, row.OriginalName, bytes);
    }

    public async Task DeleteStaffFileAsync(int id, CancellationToken ct = default)
    {
        var row = await db.Set<StaffFile>().FirstOrDefaultAsync(f => f.Id == id, ct) ?? throw HttpException.NotFound("File not found");
        db.Set<StaffFile>().Remove(row);
        await db.SaveChangesAsync(ct);
        await storage.RemoveAsync([row.StoredName], ct);
    }
}
