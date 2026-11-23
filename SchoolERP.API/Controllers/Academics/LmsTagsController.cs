using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolERP.API.Extensions;
using SchoolERP.Application.Interfaces.Services;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Helpers;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Modules.Academics;
using SchoolERP.Domain.Entities;
using SchoolERP.Contracts.Academics;
using SchoolERP.Contracts.Common;

namespace SchoolERP.API.Controllers;

/// <summary>/api/lms-tags (crudRouter over LmsTag, listed by name).</summary>
[Route("api/lms-tags")]
public sealed class LmsTagsController(ICrudService<LmsTag> crud) : CrudController<LmsTag, LmsTagRequestDto>(crud)
{
    protected override string[] Fields => ["name"];

    protected override async Task<object> ListAsync(CancellationToken ct) =>
        await Crud.ListOrderedByAsync(t => t.Name, ct);
}
