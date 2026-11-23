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

/// <summary>/api/hr/departments (crudRouter: name, headStaffId; every staff role reads, an admin writes).</summary>
[Route("api/hr/departments")]
public sealed class DepartmentsController(ICrudService<Department> crud, IHrService hr) : CrudController<Department, DepartmentRequestDto>(crud)
{
    protected override string[] Fields => ["name", "headStaffId"];
    protected override string[]? WriteRoles => ["admin"];
    protected override async Task<object> ListAsync(CancellationToken ct) => await hr.DepartmentsAsync(ct);
}
