using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence;

/// <param name="ChildType">The child entity.</param>
/// <param name="ChildTable">Its table.</param>
/// <param name="ForeignKeyColumn">The column on the child pointing at the parent's id.</param>
/// <param name="Rule">What a delete of the parent does.</param>
/// <param name="ChildHardDeletes">The child table keeps real deletes (no delstatus filter).</param>
internal sealed record ChildRule(Type ChildType, string ChildTable, string ForeignKeyColumn, DeleteRule Rule, bool ChildHardDeletes);
