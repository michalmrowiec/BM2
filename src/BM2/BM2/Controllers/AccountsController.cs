using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Account;
using BM2.Shared.Requests.Queries.Account;
using BM2.Shared.Requests.Wallet;
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
    public async Task<ActionResult<AccountDTO>> AddAccount([FromBody] AddUpdateAccountCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }

    [HttpPut]
    public async Task<ActionResult<AccountDTO>> UpdateAccount([FromBody] AddUpdateAccountCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }
    
    [HttpGet]
    public async Task<ActionResult<IList<AccountDTO>>> GetAllAccounts()
    {
        var result = await mediator.Send(new GetAllAccountsForUserQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }
}