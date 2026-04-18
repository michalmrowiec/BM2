using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Record;
using BM2.Shared.Requests.Queries.Record;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PeriodicRecordDefinitionsController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PeriodicRecordDefinitionDTO>> Add([FromBody] AddPeriodicRecordDefinitionCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }

    [HttpPut]
    public async Task<ActionResult<PeriodicRecordDefinitionDTO>> Update([FromBody] UpdatePeriodicRecordDefinitionCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeletePeriodicRecordDefinitionCommand
        {
            Id = id,
            OwnedByUserId = userContextService.UserId
        });

        return result.HandleOkResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<IList<PeriodicRecordDefinitionDTO>>> GetAll()
    {
        var result = await mediator.Send(new GetAllPeriodicRecordDefinitionsQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }
}
