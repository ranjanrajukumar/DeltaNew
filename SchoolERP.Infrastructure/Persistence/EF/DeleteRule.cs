using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence;

/// <summary>What deleting a parent does to a child row (the Node associations' onDelete).</summary>
internal enum DeleteRule
{
    CASCADE,
    SET_NULL,
    RESTRICT,
}
