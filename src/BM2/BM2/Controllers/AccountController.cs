using BM2.Application.DTOs;
using BM2.Application.Functions;
using BM2.Application.Functions.Accounts.Commands.AddAccountCommand;
using BM2.Application.Functions.Wallets.Commands.AddWalletCommand;
using BM2.Application.Functions.Wallets.Queries.GetAllWalletsForUserQuery;
using BM2.Application.Functions.Wallets.Queries.GetWalletByIdQuery;
using BM2.Controllers.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AccountController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AccountDTO>> AddAccount([FromBody] AddAccountCommand command)
    {
        command.OwnedByUserId = userContextService.GetUserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }
}