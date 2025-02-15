using BM2.Application.DTOs;
using BM2.Application.Functions;
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
public class WalletController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WalletDTO>> AddWallet([FromBody] AddWalletCommand addWalletCommand)
    {
        addWalletCommand.OwnedByUserId = userContextService.GetUserId;

        var result = await mediator.Send(addWalletCommand);

        return result.HandleCreatedResult(this, "");
    }

    [HttpGet("{walletId:guid}")]
    public async Task<ActionResult<WalletDTO>> GetWalletById([FromRoute] Guid walletId)
    {
        var result = await mediator.Send(new GetWalletByIdQuery(walletId, userContextService.GetUserId));

        return result.HandleOkResult(this);
    }

    [HttpGet("all")]
    public async Task<ActionResult<IList<WalletDTO>>> GetAllWallets()
    {
        var result = await mediator.Send(new GetAllWalletsForUserQuery(userContextService.GetUserId));

        return result.HandleOkResult(this);
    }
}