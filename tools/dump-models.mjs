// Loads the Node app's Sequelize models (no DB connection is made) and dumps the schema as JSON.
// Usage: node dump-models.mjs <path-to-server> <out.json>
import { pathToFileURL } from 'node:url';
import path from 'node:path';
import fs from 'node:fs';

const [serverDir, outFile] = process.argv.slice(2);
const models = await import(pathToFileURL(path.join(serverDir, 'src/models/index.js')).href);
const { sequelize } = models;

const typeInfo = (t) => {
  const key = t.key ?? t.constructor?.key;
  const o = t.options ?? {};
  return {
    key,
    length: o.length ?? t._length ?? null,
    precision: o.precision ?? t._precision ?? null,
    scale: o.scale ?? t._scale ?? null,
    values: t.values ?? o.values ?? null,
    unsigned: Boolean(o.unsigned ?? t._unsigned),
  };
};

const ser = (v) => {
  if (v === undefined) return undefined;
  if (v === null) return null;
  if (typeof v === 'object' && v.constructor?.name === 'NOW') return { fn: 'NOW' };
  if (typeof v === 'object' && v.fn) return { fn: v.fn };
  if (typeof v === 'object' && v.val) return { literal: v.val };
  if (typeof v === 'function') return { fn: 'js' };
  return v;
};

const out = [];
for (const M of Object.values(sequelize.models)) {
  const attrs = Object.entries(M.rawAttributes).map(([name, a]) => ({
    name,
    field: a.field ?? name,
    type: typeInfo(a.type),
    allowNull: a.allowNull !== false,
    primaryKey: Boolean(a.primaryKey),
    autoIncrement: Boolean(a.autoIncrement),
    unique: a.unique ?? false,
    defaultValue: ser(a.defaultValue),
    references: a.references ? { model: typeof a.references.model === 'string' ? a.references.model : a.references.model?.tableName, key: a.references.key } : null,
    onDelete: a.onDelete ?? null,
    comment: a.comment ?? null,
  }));
  const assocs = Object.values(M.associations).map((as) => ({
    type: as.associationType,
    as: as.as,
    target: as.target.name,
    foreignKey: as.foreignKey,
    sourceKey: as.sourceKey ?? null,
    targetKey: as.targetKey ?? null,
    onDelete: as.options.onDelete ?? null,
    constraints: as.options.constraints !== false,
    through: as.through?.model?.name ?? null,
  }));
  out.push({
    name: M.name,
    tableName: M.tableName,
    paranoid: Boolean(M.options.paranoid),
    timestamps: M.options.timestamps,
    createdAt: M._timestampAttributes?.createdAt ?? null,
    updatedAt: M._timestampAttributes?.updatedAt ?? null,
    deletedAt: M._timestampAttributes?.deletedAt ?? null,
    indexes: (M.options.indexes ?? []).map((i) => ({
      name: i.name ?? null,
      unique: Boolean(i.unique),
      fields: (i.fields ?? []).map((f) => (typeof f === 'string' ? f : f.name ?? f.attribute)),
    })),
    attributes: attrs,
    associations: assocs,
  });
}
fs.writeFileSync(outFile, JSON.stringify(out, null, 2));
console.log(`models: ${out.length}`);
await sequelize.close().catch(() => {});
