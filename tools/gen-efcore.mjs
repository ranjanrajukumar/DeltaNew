// Generates EF Core entities, configurations and metadata from schema.json (dumped from the Node app's Sequelize models).
// Usage: node gen-efcore.mjs <schema.json> <solution-dir>
import fs from 'node:fs';
import path from 'node:path';

const [schemaFile, solutionDir] = process.argv.slice(2);
const schema = JSON.parse(fs.readFileSync(schemaFile, 'utf8'));
for (const m of schema) for (const a of m.attributes) if (a.primaryKey) a.allowNull = false;
// Attributes defined from one shared object (e.g. PerformanceReview's four ratings) all read back the column name Sequelize
// wrote onto that object last; each really has its own underscored column.
const snake = (s) => s.replace(/[A-Z]/g, (c) => `_${c.toLowerCase()}`);
for (const m of schema) {
  const seen = new Set();
  for (const a of m.attributes) {
    if (seen.has(a.field)) a.field = snake(a.name);
    seen.add(a.field);
  }
}
const byName = new Map(schema.map((m) => [m.name, m]));

const AUDIT = new Set(['authadd', 'authedit', 'authdel', 'addondt', 'editondt', 'delondt', 'delstatus']);
const HARD_DELETE = new Set(['Photo', 'LoginEvent', 'AuditLog', 'PushSubscription']);

const pascal = (s) => s[0].toUpperCase() + s.slice(1);
// What System.Text.Json's camelCase policy produces for a PascalCase name.
const camelPolicy = (name) => {
  const chars = [...name];
  for (let i = 0; i < chars.length; i++) {
    if (i === 1 && chars[i] !== chars[i].toUpperCase()) break;
    const hasNext = i + 1 < chars.length;
    if (i > 0 && hasNext && chars[i + 1] !== chars[i + 1].toUpperCase()) {
      if (chars[i + 1] === ' ') chars[i] = chars[i].toLowerCase();
      break;
    }
    if (chars[i] === chars[i].toLowerCase()) break;
    chars[i] = chars[i].toLowerCase();
  }
  return chars.join('');
};
const csString = (s) => `"${String(s).replace(/\\/g, '\\\\').replace(/"/g, '\\"')}"`;

function folderOf(n) {
  const rules = [
    [/^Lms/, 'Lms'],
    [/^Hostel/, 'Hostel'],
    [/^(Transport|Vehicle|StudentTransport)/, 'Transport'],
    [/^(Library|Book)/, 'Library'],
    [/^(Fee|Payment|Refund|Expense|Income)/, 'Finance'],
    [/^(Exam|Mark$|GradeBand|GradingSetting|Revaluation|CoScholastic)/, 'Examinations'],
    [/^(Board|StudentBoardRegistration)/, 'Boards'],
    [/^(Staff|Department|Designation|LeaveRequest|LeaveType|Payslip|Job|PerformanceReview|Substitution)/, 'HR'],
    [/^(Inventory|Supplier|StockMovement|PurchaseOrder|Asset)/, 'Inventory'],
    [/^Report/, 'Reports'],
    [/^(User|Role|AuditLog|LoginEvent|AppMenuItem|PushSubscription|SupportTicket|Setting$|Branch|MasterItem|RecordExtra)/, 'Platform'],
    [/^(Admission|ApplicationDocument)/, 'Admissions'],
    [/^(Attendance|PeriodAttendance|AbsenceAlert|StudentLeave|Biometric)/, 'Attendance'],
    [/^(Student|Parent|BehaviorRecord|Photo$)/, 'Students'],
    [/^(Message|Notice|Feedback)/, 'Communication'],
    [/^(ClinicVisit|Incident|SafetyInspection)/, 'Safety'],
    [/^(OfficeVisitor|Document)/, 'Office'],
    [/^(SchoolClass|AcademicYear|Subject|ClassSubject|TimetableSlot|CalendarEvent|Homework|OnlineClass|SyllabusTopic|StudyMaterial|Room|Activity)/, 'Academics'],
  ];
  for (const [re, f] of rules) if (re.test(n)) return f;
  return 'Misc';
}

function clrType(a) {
  const t = a.type.key;
  const n = a.allowNull ? '?' : '';
  switch (t) {
    case 'INTEGER':
    case 'TINYINT':
      return 'int' + n;
    case 'BIGINT':
      return 'long' + n;
    case 'BOOLEAN':
      return 'bool' + n;
    case 'DECIMAL':
      return 'decimal' + n;
    case 'DATE':
      return 'DateTime' + n;
    case 'DATEONLY':
      return 'DateOnly' + n;
    case 'JSON':
      return 'JsonNode?';
    default:
      return 'string' + n;
  }
}

function initializer(a, prop) {
  const d = a.defaultValue;
  const t = a.type.key;
  if (d === undefined || d === null) {
    if (!a.allowNull && ['STRING', 'TEXT', 'ENUM'].includes(t)) return ' = null!;';
    return '';
  }
  if (typeof d === 'object') {
    if (d.fn === 'NOW') return t === 'DATEONLY' ? ' = DateOnly.FromDateTime(DateTime.UtcNow);' : ' = DateTime.UtcNow;';
    return !a.allowNull && ['STRING', 'TEXT', 'ENUM'].includes(t) ? ' = null!;' : '';
  }
  switch (t) {
    case 'BOOLEAN':
      return ` = ${d ? 'true' : 'false'};`;
    case 'INTEGER':
    case 'TINYINT':
    case 'BIGINT':
      return ` = ${Number(d)};`;
    case 'DECIMAL':
      return ` = ${Number(d)}m;`;
    case 'STRING':
    case 'TEXT':
    case 'ENUM':
      return ` = ${csString(d)};`;
    default:
      return '';
  }
}

function columnType(a) {
  const t = a.type;
  switch (t.key) {
    case 'STRING': {
      const len = t.length ?? 255;
      return len > 4000 ? 'nvarchar(max)' : `nvarchar(${len})`;
    }
    case 'TEXT':
    case 'JSON':
      return 'nvarchar(max)';
    case 'ENUM':
      return `nvarchar(${Math.max(20, ...t.values.map((v) => v.length))})`;
    case 'DECIMAL':
      return `decimal(${t.precision ?? 10},${t.scale ?? 2})`;
    case 'DATE':
      return 'datetime2';
    case 'DATEONLY':
      return 'date';
    default:
      return null; // INTEGER and TINYINT: plain int, matching the int property
  }
}

function dbDefaultSql(a) {
  const d = a.defaultValue;
  if (d === undefined || d === null) return null;
  const t = a.type.key;
  if (typeof d === 'object') {
    if (d.fn === 'NOW') return t === 'DATEONLY' ? 'CAST(SYSUTCDATETIME() AS date)' : 'SYSUTCDATETIME()';
    return null;
  }
  // Only defaults that equal the CLR default for value types are safe as DB defaults (EF's sentinel); strings are always safe.
  if (['STRING', 'TEXT', 'ENUM'].includes(t)) return `N'${String(d).replace(/'/g, "''")}'`;
  if (['INTEGER', 'TINYINT', 'DECIMAL'].includes(t) && Number(d) === 0) return '0';
  return null;
}

// ---------- per-model analysis ----------
const models = schema.map((m) => {
  const attrs = m.attributes.filter((a) => !AUDIT.has(a.name));
  const propOf = new Map();
  const used = new Set(['Authadd', 'Authedit', 'Authdel', 'Addondt', 'Editondt', 'Delondt', 'Delstatus']);
  for (const a of attrs) {
    let p = pascal(a.name);
    if (p === m.name) p = p + 'Value';
    propOf.set(a.name, p);
    used.add(p);
  }
  const navs = [];
  const seenAs = new Set();
  for (const as of m.associations) {
    if (seenAs.has(as.as)) continue;
    seenAs.add(as.as);
    if (as.type === 'BelongsTo' && !as.constraints) continue; // no FK in the database: no navigation
    let p = pascal(as.as);
    if (used.has(p) || p === m.name) p = p + 'Navigation';
    used.add(p);
    navs.push({ ...as, prop: p });
  }
  return { ...m, attrs, propOf, navs, folder: folderOf(m.name) };
});
const modelByName = new Map(models.map((m) => [m.name, m]));
const attrByNameOrField = (m, key) => m.attributes.find((a) => a.name === key || a.field === key);

// ---------- entities ----------
function entityFile(m) {
  const L = [];
  L.push('// Ported from the Node app\'s Sequelize model ' + m.name + ' (first generated by tools/gen-efcore.mjs).');
  L.push('using System.Text.Json.Nodes;');
  L.push('using System.Text.Json.Serialization;');
  L.push('using SchoolERP.Domain.Common;');
  L.push('');
  L.push('namespace SchoolERP.Domain.Entities;');
  L.push('');
  const ifaces = ['AuditableEntity'];
  const createdAt = m.createdAt && m.attributes.find((a) => a.name === m.createdAt);
  const updatedAt = m.updatedAt && m.attributes.find((a) => a.name === m.updatedAt);
  if (createdAt) ifaces.push('IHasCreatedAt');
  if (updatedAt) ifaces.push('IHasUpdatedAt');
  if (HARD_DELETE.has(m.name)) ifaces.push('IHardDelete');
  if (m.paranoid) ifaces.push('IHasDeletedAt');
  L.push(`/// <summary>Table <c>${m.tableName}</c>.</summary>`);
  L.push(`public class ${m.name} : ${ifaces.join(', ')}`);
  L.push('{');
  for (const a of m.attrs) {
    const p = m.propOf.get(a.name);
    let type = clrType(a);
    // timestamp interfaces need non-nullable / nullable shapes
    if (createdAt && a.name === m.createdAt) type = 'DateTime';
    if (updatedAt && a.name === m.updatedAt) type = 'DateTime';
    if (m.paranoid && a.name === m.deletedAt) type = 'DateTime?';
    if (a.comment) L.push(`    /// <summary>${a.comment}</summary>`);
    if (a.name === 'passwordHash') L.push('    [JsonIgnore] // never sent to a client');
    else if (camelPolicy(p) !== a.name) L.push(`    [JsonPropertyName(${csString(a.name)})]`);
    L.push(`    public ${type} ${p} { get; set; }${initializer(a, p)}`);
  }
  // interface bridges where the column is not literally CreatedAt/UpdatedAt/DeletedAt
  if (createdAt && m.propOf.get(m.createdAt) !== 'CreatedAt')
    L.push(`    DateTime IHasCreatedAt.CreatedAt { get => ${m.propOf.get(m.createdAt)}; set => ${m.propOf.get(m.createdAt)} = value; }`);
  if (updatedAt && m.propOf.get(m.updatedAt) !== 'UpdatedAt')
    L.push(`    DateTime IHasUpdatedAt.UpdatedAt { get => ${m.propOf.get(m.updatedAt)}; set => ${m.propOf.get(m.updatedAt)} = value; }`);
  if (m.paranoid && m.propOf.get(m.deletedAt) !== 'DeletedAt')
    L.push(`    DateTime? IHasDeletedAt.DeletedAt { get => ${m.propOf.get(m.deletedAt)}; set => ${m.propOf.get(m.deletedAt)} = value; }`);
  if (m.navs.length) L.push('');
  for (const n of m.navs) {
    if (n.type === 'HasMany') {
      L.push('    [JsonIgnore]');
      L.push(`    public ICollection<${n.target}> ${n.prop} { get; set; } = new List<${n.target}>();`);
    } else {
      L.push(`    [JsonPropertyName(${csString(n.as)})]`);
      L.push('    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]');
      L.push(`    public ${n.target}? ${n.prop} { get; set; }`);
    }
  }
  L.push('}');
  return L.join('\n') + '\n';
}

// ---------- configurations ----------
function lambdaList(m, keys, v = 'e') {
  const props = keys.map((k) => {
    const a = attrByNameOrField(m, k);
    if (!a) throw new Error(`${m.name}: unknown index field ${k}`);
    return AUDIT.has(a.name) ? pascal(a.name) : m.propOf.get(a.name);
  });
  return props.length === 1 ? `${v} => ${v}.${props[0]}` : `${v} => new { ${props.map((p) => `${v}.${p}`).join(', ')} }`;
}

function configFile(m) {
  const L = [];
  L.push('// Ported from the Node app\'s Sequelize model ' + m.name + ' (first generated by tools/gen-efcore.mjs).');
  L.push('using Microsoft.EntityFrameworkCore;');
  L.push('using Microsoft.EntityFrameworkCore.Metadata.Builders;');
  L.push('using SchoolERP.Domain.Entities;');
  L.push('');
  L.push('namespace SchoolERP.Infrastructure.Persistence.Configurations;');
  L.push('');
  L.push(`internal sealed class ${m.name}Configuration : IEntityTypeConfiguration<${m.name}>`);
  L.push('{');
  L.push(`    public void Configure(EntityTypeBuilder<${m.name}> builder)`);
  L.push('    {');
  const checks = m.attrs.filter((a) => a.type.key === 'ENUM');
  if (checks.length) {
    L.push(`        builder.ToTable(${csString(m.tableName)}, t =>`);
    L.push('        {');
    for (const a of checks) {
      L.push(`            t.HasCheckConstraint(${csString(`CK_${m.tableName}_${a.field}`)}, ${csString(`[${a.field}] IN (${a.type.values.map((v) => `N'${v.replace(/'/g, "''")}'`).join(', ')})`)});`);
    }
    L.push('        });');
  } else {
    L.push(`        builder.ToTable(${csString(m.tableName)});`);
  }
  L.push('        builder.ConfigureAudit();');
  if (HARD_DELETE.has(m.name)) {
    // no "not deleted" filter: these rows are really deleted
  } else if (m.paranoid) {
    L.push(`        builder.HasQueryFilter(e => e.${m.propOf.get(m.deletedAt)} == null);`);
  } else {
    L.push('        builder.HasQueryFilter(e => !e.Delstatus);');
  }

  const pk = m.attrs.filter((a) => a.primaryKey);
  L.push(`        builder.HasKey(${lambdaList(m, pk.map((a) => a.name))});`);
  for (const a of m.attrs) {
    const p = m.propOf.get(a.name);
    const parts = [`builder.Property(e => e.${p}).HasColumnName(${csString(a.field)})`];
    const ct = columnType(a);
    if (ct) parts.push(`.HasColumnType(${csString(ct)})`);
    if (a.type.key === 'STRING') parts.push(`.HasMaxLength(${a.type.length ?? 255})`);
    if (!a.allowNull && ['STRING', 'TEXT', 'ENUM'].includes(a.type.key)) parts.push('.IsRequired()');
    if (a.primaryKey && a.autoIncrement) parts.push('.UseIdentityColumn()');
    else if (a.primaryKey && ['INTEGER', 'BIGINT'].includes(a.type.key)) parts.push('.ValueGeneratedNever()');
    const def = a.primaryKey ? null : dbDefaultSql(a);
    if (def) parts.push(`.HasDefaultValueSql(${csString(def)})`);
    if (a.type.key === 'JSON') parts.push('.HasConversion(JsonNodeConversion.Converter, JsonNodeConversion.Comparer)');
    L.push('        ' + parts.join('') + ';');
  }

  // unique columns and named unique keys
  const uniqueGroups = new Map();
  for (const a of m.attrs) {
    if (!a.unique || a.primaryKey) continue;
    const key = a.unique === true ? `__${a.name}` : String(a.unique);
    if (!uniqueGroups.has(key)) uniqueGroups.set(key, []);
    uniqueGroups.get(key).push(a.name);
  }
  for (const [key, fields] of uniqueGroups) {
    const named = key.startsWith('__') ? '' : `.HasDatabaseName(${csString(key)})`;
    L.push(`        builder.HasIndex(${lambdaList(m, fields)}).IsUnique()${named};`);
  }
  for (const idx of m.indexes) {
    const named = idx.name ? `.HasDatabaseName(${csString(idx.name)})` : '';
    L.push(`        builder.HasIndex(${lambdaList(m, idx.fields)})${idx.unique ? '.IsUnique()' : ''}${named};`);
  }

  // relationships: configured from the dependent (belongsTo) side; parents' hasMany/hasOne without a belongsTo are added from the parent side
  for (const n of m.navs.filter((x) => x.type === 'BelongsTo')) {
    const target = modelByName.get(n.target);
    const fkProp = m.propOf.get(n.foreignKey);
    if (!fkProp) throw new Error(`${m.name}: FK ${n.foreignKey} not found`);
    const inv = target.navs.find((x) => (x.type === 'HasMany' || x.type === 'HasOne') && x.target === m.name && x.foreignKey === n.foreignKey);
    let line = `        builder.HasOne(e => e.${n.prop})`;
    if (inv?.type === 'HasOne') line += `.WithOne(p => p.${inv.prop}).HasForeignKey<${m.name}>(e => e.${fkProp})`;
    else line += `.WithMany(${inv ? `p => p.${inv.prop}` : ''}).HasForeignKey(e => e.${fkProp})`;
    if (n.targetKey && n.targetKey !== 'id') line += `.HasPrincipalKey${inv?.type === 'HasOne' ? `<${n.target}>` : ''}(p => p.${target.propOf.get(n.targetKey)})`;
    line += '.OnDelete(DeleteBehavior.NoAction);';
    L.push(line);
  }
  for (const n of m.navs.filter((x) => x.type === 'HasMany' || x.type === 'HasOne')) {
    const child = modelByName.get(n.target);
    const back = child.navs.find((x) => x.type === 'BelongsTo' && x.target === m.name && x.foreignKey === n.foreignKey);
    if (back) continue;
    const fkProp = child.propOf.get(n.foreignKey);
    if (!fkProp) throw new Error(`${m.name}->${child.name}: FK ${n.foreignKey} not found`);
    if (n.type === 'HasMany')
      L.push(`        builder.HasMany(e => e.${n.prop}).WithOne().HasForeignKey(c => c.${fkProp}).OnDelete(DeleteBehavior.NoAction);`);
    else
      L.push(`        builder.HasOne(e => e.${n.prop}).WithOne().HasForeignKey<${child.name}>(c => c.${fkProp}).OnDelete(DeleteBehavior.NoAction);`);
  }
  L.push('    }');
  L.push('}');
  return L.join('\n') + '\n';
}

// ---------- metadata (delete rules, unique sets, DbSets) ----------
function metadataFile() {
  const L = [];
  L.push('#nullable enable'); L.push('// Generated by tools/gen-efcore.mjs from the Node app\'s Sequelize associations: what a delete does to child rows, and the unique value sets per table.');
  L.push('using SchoolERP.Domain.Entities;');
  L.push('');
  L.push('namespace SchoolERP.Infrastructure.Persistence;');
  L.push('');
  L.push('internal static partial class EntityMetadata');
  L.push('{');
  L.push('    /// <summary>Parent type -> children and the rule the Node app applied on delete (CASCADE, SET NULL or RESTRICT).</summary>');
  L.push('    public static readonly IReadOnlyDictionary<Type, ChildRule[]> DeleteRules = new Dictionary<Type, ChildRule[]>');
  L.push('    {');
  for (const m of models) {
    const rules = m.associations
      .filter((as) => as.type === 'HasMany' || as.type === 'HasOne')
      .filter((as) => !as.sourceKey || as.sourceKey === 'id')
      .map((as) => {
        const c = modelByName.get(as.target);
        const fk = attrByNameOrField(c, as.foreignKey);
        const rule = String(as.onDelete ?? 'SET NULL').toUpperCase().replace(' ', '_');
        return `new(typeof(${c.name}), ${csString(c.tableName)}, ${csString(fk.field)}, DeleteRule.${rule === 'NO_ACTION' ? 'RESTRICT' : rule}, ${HARD_DELETE.has(c.name) ? 'true' : 'false'})`;
      });
    if (rules.length) L.push(`        [typeof(${m.name})] = [${rules.join(', ')}],`);
  }
  L.push('    };');
  L.push('');
  L.push('    /// <summary>Column sets that must be unique per table (a soft-deleted row holding one is purged before a new row reuses it).</summary>');
  L.push('    public static readonly IReadOnlyDictionary<Type, string[][]> UniqueSets = new Dictionary<Type, string[][]>');
  L.push('    {');
  for (const m of models) {
    if (HARD_DELETE.has(m.name) || m.paranoid) continue;
    const sets = [];
    const groups = new Map();
    for (const a of m.attrs) {
      if (!a.unique || a.primaryKey) continue;
      const key = a.unique === true ? `__${a.name}` : String(a.unique);
      if (!groups.has(key)) groups.set(key, []);
      groups.get(key).push(a.field);
    }
    sets.push(...groups.values());
    for (const idx of m.indexes) if (idx.unique) sets.push(idx.fields.map((f) => attrByNameOrField(m, f).field));
    const pk = m.attrs.filter((a) => a.primaryKey);
    if (pk.length && pk.every((a) => !a.autoIncrement)) sets.push(pk.map((a) => a.field));
    if (sets.length)
      L.push(`        [typeof(${m.name})] = [${sets.map((s) => `[${s.map(csString).join(', ')}]`).join(', ')}],`);
  }
  L.push('    };');
  L.push('');
  L.push('    public static readonly IReadOnlyDictionary<Type, string> TableNames = new Dictionary<Type, string>');
  L.push('    {');
  for (const m of models) L.push(`        [typeof(${m.name})] = ${csString(m.tableName)},`);
  L.push('    };');
  L.push('}');
  return L.join('\n') + '\n';
}

function dbSetsFile() {
  const L = [];
  L.push('#nullable enable'); L.push('// Generated by tools/gen-efcore.mjs: one DbSet per Node model.');
  L.push('using Microsoft.EntityFrameworkCore;');
  L.push('using SchoolERP.Domain.Entities;');
  L.push('');
  L.push('namespace SchoolERP.Infrastructure.Persistence;');
  L.push('');
  L.push('public partial class SchoolErpDbContext');
  L.push('{');
  for (const m of models) L.push(`    public DbSet<${m.name}> ${m.name}Set => Set<${m.name}>();`);
  L.push('}');
  return L.join('\n') + '\n';
}

// ---------- write ----------
const write = (rel, content) => {
  const full = path.join(solutionDir, rel);
  fs.mkdirSync(path.dirname(full), { recursive: true });
  fs.writeFileSync(full, content);
};
for (const m of models) {
  write(`SchoolERP.Domain/Entities/${m.folder}/${m.name}.cs`, entityFile(m));
  write(`SchoolERP.Infrastructure/Persistence/Configurations/${m.folder}/${m.name}Configuration.cs`, configFile(m));
}
write('SchoolERP.Infrastructure/Persistence/EntityMetadata.g.cs', metadataFile());
write('SchoolERP.Infrastructure/Persistence/SchoolErpDbContext.DbSets.g.cs', dbSetsFile());
console.log(`entities: ${models.length}`);
const folders = {};
for (const m of models) folders[m.folder] = (folders[m.folder] || 0) + 1;
console.log(folders);

