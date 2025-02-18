﻿using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class RecordsController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecordDTO>> AddTag([FromBody] AddRecordCommand command)
    {
        command.OwnedByUserId = userContextService.GetUserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }
}