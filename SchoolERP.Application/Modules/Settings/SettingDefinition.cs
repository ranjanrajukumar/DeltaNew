using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Modules.Settings;

/// <summary>
/// One school-wide setting: its group, label, type (text | integer | number), limits and default. A text setting may list
/// options (it must then be one of them, or blank when not required), a pattern (checked when something is entered) and required.
/// </summary>
public sealed record SettingDefinition(
    string Key, string Group, string Label, string Type, Func<JsonNode?> Default,
    double? Min = null, double? Max = null, string[]? Options = null, bool Required = false,
    Regex? Pattern = null, string? PatternHelp = null, string? Help = null);
