using BM2.Application.DTOs;
using BM2.Application.Functions;
using BM2.Application.Functions.Category.Commands.Requests;
using BM2.Application.Functions.Record.Commands.Requests;
using BM2.Application.Functions.Tag.Commands.Requests;
using BM2.Application.Functions.Wallet.Commands.Requests;
using BM2.Application.Functions.Wallet.Queries.Requests;
using BM2.Controllers.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class RecordController(
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