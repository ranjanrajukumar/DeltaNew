using Microsoft.Extensions.Configuration;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.FileStorage;

/// <summary>The upload folder (Storage:UploadDir, default "uploads", as the Node app's UPLOAD_DIR).</summary>
public sealed class LocalFileStorage(IConfiguration configuration) : IFileStorage
{
    private string UploadDir => Path.GetFullPath(configuration["Storage:UploadDir"] ?? "uploads");

    /// <summary>The full path of a stored name, or null when it would leave the upload folder.</summary>
    private string? PathOf(string name)
    {
        // stored names are generated hex names; never follow anything that leaves the folder
        var path = Path.GetFullPath(Path.Combine(UploadDir, name));
        return path.StartsWith(UploadDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? path : null;
    }

    public async Task<byte[]?> ReadAsync(string storedName, CancellationToken ct = default)
    {
        if (PathOf(storedName) is not { } path) return null;
        try
        {
            return await File.ReadAllBytesAsync(path, ct);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    public Task RemoveAsync(IEnumerable<string> storedNames, CancellationToken ct = default)
    {
        foreach (var name in storedNames)
        {
            if (PathOf(name) is not { } path) continue;
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // already gone or in use: nothing to clean up
            }
        }
        return Task.CompletedTask;
    }
}
