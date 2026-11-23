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

/// <summary>/api/notices (crudRouter over Notice: title, body, audience).</summary>
[Route("api/notices")]
public sealed class NoticesController(ICrudService<Notice> crud) : CrudController<Notice, NoticeRequestDto>(crud)
{
    protected override string[] Fields => ["title", "body", "audience"];
}
