using System.Text;
using System.Text.Json;
using SchoolERP.API.Middleware;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Files;

namespace SchoolERP.API.Extensions;

/// <summary>
/// Uploads sent as the raw request body (the Node app's express.raw), and file downloads. Sending the bytes directly avoids a
/// multipart parser; the file's real type is checked from its content.
/// </summary>
public static class RawUpload
{
    /// <summary>The whole body, refusing anything over <paramref name="limit"/> bytes with the Node app's 413.</summary>
    public static async Task<byte[]> ReadBodyAsync(this HttpRequest request, int limit, CancellationToken ct)
    {
        if (request.Body.CanSeek) request.Body.Position = 0; // the audit trail may have read it already
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await request.Body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > limit) throw new HttpException(413, "Request body is too large for this endpoint");
            buffer.Write(chunk, 0, read);
        }
        return buffer.ToArray();
    }

    /// <summary>Records these fields in the audit log instead of the uploaded bytes.</summary>
    public static void AuditAs(this HttpContext context, object details) =>
        context.Items[ApiAccessMiddleware.AuditDetailsKey] = JsonSerializer.Serialize(details, JsonDefaults.Options);

    /// <summary>encodeURIComponent: like Uri.EscapeDataString but leaving ! ' ( ) * as they are.</summary>
    public static string EncodeUriComponent(string text)
    {
        var escaped = new StringBuilder(Uri.EscapeDataString(text));
        foreach (var (code, ch) in new[] { ("%21", "!"), ("%27", "'"), ("%28", "("), ("%29", ")"), ("%2A", "*") }) escaped.Replace(code, ch);
        return escaped.ToString();
    }

    /// <summary>Sends a stored file inline, with no type sniffing by the browser.</summary>
    public static IResult File(HttpResponse response, FilesService.Download file, string cacheControl)
    {
        response.Headers.ContentDisposition = $"inline; filename*=UTF-8''{EncodeUriComponent(file.OriginalName)}";
        response.Headers.XContentTypeOptions = "nosniff";
        response.Headers.CacheControl = cacheControl;
        return Results.Bytes(file.Bytes, file.MimeType);
    }
}
