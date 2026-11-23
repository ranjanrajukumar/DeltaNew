using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Boards;

/// <summary>A rule version in force: its payload (the rule itself) plus where it came from.</summary>
public sealed record RuleHit(int Id, int BoardId, int Version, bool Verified, string? SourceRef, int? EffectiveYearId, JsonNode? Payload);
