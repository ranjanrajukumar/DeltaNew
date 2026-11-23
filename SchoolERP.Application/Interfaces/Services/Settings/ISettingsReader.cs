using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Modules.Messaging;
using SchoolERP.Application.Modules.Settings;
using SchoolERP.Application.Modules.Students;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Interfaces.Services;

/// <summary>School-wide settings: a saved override, else the setting's default (the Node app's settings/settings.service.js).</summary>
public interface ISettingsReader
{
    Task<JsonNode?> GetAsync(string key, CancellationToken ct = default);

    Task<string> GetStringAsync(string key, CancellationToken ct = default);

    /// <summary>A numeric setting (Number(value)); 0 when it is not a number.</summary>
    Task<double> GetNumberAsync(string key, CancellationToken ct = default);

    /// <summary>A 1/0 switch read with JavaScript truthiness.</summary>
    Task<bool> GetFlagAsync(string key, CancellationToken ct = default);

    /// <summary>Every setting with its current value, default and whether it has been changed, for the settings page.</summary>
    Task<List<object>> DescribeAllAsync(CancellationToken ct = default);

    /// <summary>Validates the whole batch first, then saves it, so a bad value changes nothing.</summary>
    Task SaveAsync(JsonObject? values, CancellationToken ct = default);

    /// <summary>Puts a setting back to its default by removing the saved override.</summary>
    Task ResetAsync(string key, CancellationToken ct = default);

    /// <summary>The school's printable identity, used on certificates, ID cards and messages.</summary>
    Task<SettingsReader.SchoolProfile> SchoolProfileAsync(CancellationToken ct = default);

    /// <summary>A raw setting row's value (JSON text), also for keys outside the definitions (e.g. push.vapid).</summary>
    Task<string?> RawAsync(string key, CancellationToken ct = default);
}
