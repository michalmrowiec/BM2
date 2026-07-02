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
public class RecordTemplatesController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecordTemplateDTO>> AddRecordTemplate([FromBody] AddRecordTemplateCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }

    [HttpPut]
    public async Task<ActionResult<RecordTemplateDTO>> UpdateRecordTemplate([FromBody] UpdateRecordTemplateCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRecordTemplate(Guid id)
    {
        var result = await mediator.Send(new DeleteRecordTemplateCommand
        {
            Id = id,
            OwnedByUserId = userContextService.UserId
        });

        return result.HandleOkResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<IList<RecordTemplateDTO>>> GetAllForGrid()
    {
        var result = await mediator.Send(new GetAllRecordTemplatesQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }

    [HttpGet("~/api/v1/wallets/{walletId:guid}/recordTemplates")]
    public async Task<ActionResult<IList<RecordTemplateDTO>>> GetAllForWallet(Guid walletId)
    {
        var result = await mediator.Send(new GetAllRecordTemplatesQuery(userContextService.UserId));

        if (result.Status != BM2.Application.Responses.BaseResponse.ResponseStatus.Success)
            return result.HandleOkResult(this);

        return Ok(result.ReturnedObj?.Where(x => x.WalletId == walletId));
    }
}
