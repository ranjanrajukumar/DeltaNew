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

/// <summary>/api/my-class: a class teacher's own classes (the Node app's academics/myClass.routes.js).</summary>
[ApiController]
[Route("api/my-class")]
public sealed class MyClassController(IMyClassService myClass, IClassService classes, ICurrentUser user) : ControllerBase
{
    /// <summary>The classes the signed-in teacher is class teacher of.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await classes.ClassesOfAsync(user.StaffId, ct));

    /// <summary>That class today. A teacher can open only their own classes; an admin can open any.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        if (user.Role != "admin" && !(await classes.ClassIdsOfAsync(user.StaffId, ct)).Contains(id))
            throw HttpException.Forbidden("You are not the class teacher of this class");
        return Ok(await myClass.MyClassAsync(id, ct) ?? throw HttpException.NotFound("Class not found"));
    }
}
