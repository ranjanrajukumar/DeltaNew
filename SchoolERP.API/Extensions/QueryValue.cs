namespace SchoolERP.API.Extensions;

/// <summary>Query-string values read the way the Node handlers used req.query.</summary>
public static class QueryValue
{
    /// <summary>
    /// An id filter: null (no filter) when missing or empty, the number when it is one, and -1 (matches nothing) otherwise:
    /// MySQL compared an int column with a non-numeric string and found no rows.
    /// </summary>
    public static int? Id(string? value) =>
        string.IsNullOrEmpty(value) ? null : int.TryParse(value.Trim(), out var n) ? n : -1;
}
