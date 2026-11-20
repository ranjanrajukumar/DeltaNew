using System.Reflection;
using System.Text.Json.Nodes;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Interfaces.Services;

namespace SchoolERP.Application.Common.Crud;

/// <summary>
/// The Node app's utils/crud.js for one table: list, create, update and delete, writing only a fixed whitelist of fields
/// (never whatever the request sends). A blank value ('') is stored as NULL so optional columns are cleared rather than set to ''.
/// </summary>
public sealed class CrudService<T>(IRepository<T> repository) : ICrudService<T> where T : class, new()
{
    /// <summary>Every row, newest first (Model.findAll({ order: [['id', 'DESC']] })).</summary>
    public Task<List<T>> ListAsync(CancellationToken ct = default) => repository.ListNewestFirstAsync(ct);

    public Task<List<T>> ListOrderedByAsync<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderBy, CancellationToken ct = default) =>
        repository.ListOrderedByAsync(orderBy, ct);

    /// <summary>Creates a row from the whitelisted fields; 400 "No fields" when none was sent. Returns the new id.</summary>
    public async Task<int> CreateAsync(Body body, IReadOnlyList<string> fields, CancellationToken ct = default)
    {
        var entity = new T();
        if (CrudService.Apply(body, fields, entity) == 0) throw HttpException.BadRequest("No fields");
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return CrudService.IdOf(entity);
    }

    /// <summary>Updates the whitelisted fields sent; 400 "No fields" when none was sent; false when the row does not exist.</summary>
    public async Task<bool> UpdateAsync(int id, Body body, IReadOnlyList<string> fields, CancellationToken ct = default)
    {
        if (!fields.Any(body.Has)) throw HttpException.BadRequest("No fields");
        var entity = await repository.FindAsync(id, ct);
        if (entity is null) return false;
        CrudService.Apply(body, fields, entity);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Deletes the row if it exists (a soft delete); deleting a missing row is not an error, as in Node.</summary>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await repository.FindAsync(id, ct);
        if (entity is null) return;
        repository.Remove(entity);
        await repository.SaveChangesAsync(ct);
    }
}

/// <summary>Copying whitelisted request fields onto an entity, converted to each column's type (no database work).</summary>
public static class CrudService
{
    /// <summary>
    /// Copies each whitelisted field the body has onto the entity. With <paramref name="blankAsNull"/> (the usual pick) a ''
    /// becomes null. A null for a column that cannot be null is Sequelize's "Homework.dueDate cannot be null" (400).
    /// Returns how many fields were copied.
    /// </summary>
    public static int Apply<T>(Body body, IReadOnlyList<string> fields, T entity, bool blankAsNull = true) where T : class
    {
        var count = 0;
        foreach (var field in fields)
        {
            if (!body.Has(field)) continue;
            Set(entity, field, blankAsNull && body.IsBlank(field) ? null : body.Raw(field));
            count++;
        }
        return count;
    }

    /// <summary>Sets one property from a request value, converted to the column's type.</summary>
    public static void Set<T>(T entity, string field, JsonNode? value) where T : class
    {
        var property = PropertyOf(entity.GetType(), field);
        var converted = JsValue.ConvertTo(value, NullableOf(property.PropertyType));
        if (converted is null && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) is null)
            throw HttpException.BadRequest($"{entity.GetType().Name}.{field} cannot be null");
        property.SetValue(entity, converted);
    }

    private static Type NullableOf(Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) is null ? typeof(Nullable<>).MakeGenericType(type) : type;

    private static PropertyInfo PropertyOf(Type type, string field) =>
        type.GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
        ?? throw new InvalidOperationException($"{type.Name} has no property {field}");

    public static int IdOf(object entity) => (int)PropertyOf(entity.GetType(), "Id").GetValue(entity)!;
}
