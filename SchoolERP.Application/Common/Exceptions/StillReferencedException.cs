namespace SchoolERP.Application.Common.Exceptions;

/// <summary>A delete refused because live rows still refer to the record (the Node app's FK error 1451).</summary>
public sealed class StillReferencedException(string detail) : Exception(detail);
