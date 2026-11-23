using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.API.Filters;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Files;
using SchoolERP.Application.Modules.HR;
using SchoolERP.Contracts.Common;
using SchoolERP.Domain.Entities;

namespace SchoolERP.API.Controllers;

/// <summary>/api/hr/designations (crudRouter: name; every staff role reads, an admin writes).</summary>
[Route("api/hr/designations")]
public sealed class DesignationsController(ICrudService<Designation> crud) : CrudController<Designation, DesignationRequestDto>(crud)
{
    protected override string[] Fields => ["name"];
    protected override string[]? WriteRoles => ["admin"];
    protected override async Task<object> ListAsync(CancellationToken ct) =>
        await Crud.ListOrderedByAsync(d => d.Name, ct);
}
