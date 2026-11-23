using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Modules.Boards;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string Section = "Seed";

    /// <summary>Apply pending EF Core migrations at startup (the Node app synced its schema on every start).</summary>
    public bool MigrateOnStartup { get; set; } = true;

    public string AdminEmail { get; set; } = "admin@school.local";

    /// <summary>The first administrator's password; change it after the first sign-in.</summary>
    public string AdminPassword { get; set; } = "admin123";
}
