namespace SchoolERP.Application.Common.Exceptions;

/// <summary>
/// An error with an HTTP status (the Node app's HttpError). The error middleware turns it into { "error": message },
/// plus { "errors": [...] } when a list of validation problems is attached.
/// </summary>
public class HttpException(int status, string message, IReadOnlyList<string>? errors = null) : Exception(message)
{
    public int Status { get; } = status;
    public IReadOnlyList<string>? Errors { get; } = errors;

    /// <summary>Extra fields to put on the JSON error body (for example mustChangePassword).</summary>
    public IDictionary<string, object?>? Extra { get; init; }

    public static HttpException BadRequest(string message) => new(400, message);
    public static HttpException Unauthorized(string message) => new(401, message);
    public static HttpException Forbidden(string message) => new(403, message);
    public static HttpException NotFound(string message) => new(404, message);
    public static HttpException Conflict(string message) => new(409, message);

    /// <summary>400 with the first problem as the error and the whole list as errors.</summary>
    public static HttpException Invalid(IReadOnlyList<string> errors) => new(400, errors[0], errors);
}

/// <summary>A delete refused because live rows still refer to the record (the Node app's FK error 1451).</summary>
public sealed class StillReferencedException(string detail) : Exception(detail);
