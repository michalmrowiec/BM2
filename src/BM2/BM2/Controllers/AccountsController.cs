using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using BM2.Shared.Requests.Queries.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AccountsController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AccountDTO>> AddAccount([FromBody] AddAccountCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }

    [HttpPut]
    public async Task<ActionResult<AccountDTO>> UpdateAccount([FromBody] UpdateAccountCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAccount(Guid id)
    {
        var result = await mediator.Send(new DeleteAccountCommand
        {
            Id = id,
            OwnedByUserId = userContextService.UserId
        });

        return result.HandleOkResult(this);
    }

    [HttpGet]
    [Route("")]
    [Route("all")]
    public async Task<ActionResult<IList<AccountDTO>>> GetAllAccounts()
    {
        var result = await mediator.Send(new GetAllAccountsForUserQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }

    [HttpGet]
    [Route("active")]
    public async Task<ActionResult<IList<AccountDTO>>> GetActiveAccounts()
    {
        var result = await mediator.Send(new GetAllAccountsForUserQuery(userContextService.UserId, ActiveOnly: true));

        return result.HandleOkResult(this);
    }
}
