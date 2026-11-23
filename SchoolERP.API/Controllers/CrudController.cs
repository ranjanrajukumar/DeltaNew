using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Helpers;

namespace SchoolERP.API.Controllers;

/// <summary>
/// The standard list/create/update/delete endpoints for one table (the Node app's crudRouter). A derived controller sets its
/// route, the writable <see cref="Fields"/>, and overrides <see cref="ListAsync"/> when the list needs joined data.
/// </summary>
[ApiController]
public abstract class CrudController<T, TRequest>(ICrudService<T> crud) : ControllerBase where T : class, new()
{
    protected ICrudService<T> Crud => crud;

    /// <summary>The writable fields, fixed in code.</summary>
    protected abstract string[] Fields { get; }

    /// <summary>The Node app's writeRoles(...) on the router: who may create, update and delete. Null = whoever reaches the route.</summary>
    protected virtual string[]? WriteRoles => null;

    private void CheckWrite()
    {
        if (WriteRoles is { } roles
            && !HttpContext.RequestServices.GetRequiredService<Application.Common.Interfaces.ICurrentUser>().IsInRole(roles))
            throw new Application.Common.Exceptions.HttpException(403, "Forbidden");
    }

    /// <summary>What GET / returns; every row newest first unless overridden.</summary>
    protected virtual async Task<object> ListAsync(CancellationToken ct) => await crud.ListAsync(ct);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await ListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TRequest request, CancellationToken ct)
    {
        CheckWrite();
        return StatusCode(201, new { id = await crud.CreateAsync(RequestDtoMapper.ToBody(request), Fields, ct) });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TRequest request, CancellationToken ct)
    {
        CheckWrite();
        return await crud.UpdateAsync(id, RequestDtoMapper.ToBody(request), Fields, ct) ? Ok(new { ok = true }) : NotFound(new { error = "Not found" });
    }

    /// <summary>What DELETE /:id does; a plain delete unless overridden (a missing row is not an error).</summary>
    protected virtual Task DeleteAsync(int id, CancellationToken ct) => crud.DeleteAsync(id, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        CheckWrite();
        await DeleteAsync(id, ct);
        return Ok(new { ok = true });
    }
}
