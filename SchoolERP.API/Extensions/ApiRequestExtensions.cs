using System.Security.Claims;

namespace SchoolERP.API.Extensions;

/// <summary>Helpers shared by the middleware and filters: the request's API address and the signed-in user's id claim.</summary>
public static class ApiRequestExtensions
{
    /// <summary>Areas anyone may call without signing in (the auth endpoints that need a user check it themselves).</summary>
    public static readonly HashSet<string> OpenAreas = ["health", "auth", "public", "device"];

    /// <summary>The request path, e.g. /api/students/3.</summary>
    public static string ApiPath(this HttpRequest request) => request.Path.Value ?? "";

    /// <summary><see cref="ApiPath"/> plus the query string.</summary>
    public static string ApiUrl(this HttpRequest request) => request.ApiPath() + request.QueryString.Value;

    /// <summary>The area of an /api/... path: "students" for /api/students/3; "" when there is none.</summary>
    public static string ApiArea(string apiPath) =>
        apiPath.Split('/', StringSplitOptions.RemoveEmptyEntries) is { Length: > 1 } parts ? parts[1] : "";

    /// <summary>The "id" claim of a token JwtBearer accepted, or null when the request has no valid token.</summary>
    public static int? UserId(this ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true && int.TryParse(principal.FindFirst("id")?.Value, out var id) ? id : null;
}
