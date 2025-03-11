using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.Requests.Queries.Record;
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
    public async Task<ActionResult<RecordDTO>> AddRecord([FromBody] AddRecordCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }
    
    [HttpPut]
    public async Task<ActionResult<RecordDTO>> UpdateRecord([FromBody] UpdateRecordCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpPatch]
    public async Task<ActionResult> UpdateRecordAccount([FromBody] UpdateAccountAssignmentCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<IList<RecordDTO>>> GetRecord([FromQuery] int year, [FromQuery] int month)
    {
        var result = await mediator.Send(new GetRecordsForMonthQuery(userContextService.UserId, year, month));

        return result.HandleOkResult(this);
    }
}