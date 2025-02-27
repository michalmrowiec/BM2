using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Wallet;
using BM2.Shared.Requests.Queries.Account;
using BM2.Shared.Requests.Queries.Category;
using BM2.Shared.Requests.Queries.Tag;
using BM2.Shared.Requests.Wallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class WalletsController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WalletDTO>> AddWallet([FromBody] AddWalletCommand addWalletCommand)
    {
        addWalletCommand.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(addWalletCommand);

        return result.HandleCreatedResult(this, "");
    }

    [HttpGet("{walletId:guid}")]
    public async Task<ActionResult<WalletDTO>> GetWalletById([FromRoute] Guid walletId)
    {
        var result = await mediator.Send(new GetWalletByIdQuery(walletId, userContextService.UserId));

        return result.HandleOkResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<IList<WalletDTO>>> GetAllWallets()
    {
        var result = await mediator.Send(new GetAllWalletsForUserQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }

    [HttpGet("{walletId:guid}/accounts")]
    public async Task<ActionResult<IList<WalletDTO>>> GetAccountsForWallet([FromRoute] Guid walletId)
    {
        var result = await mediator.Send(new GetAccountsForWalletByIdQuery(walletId, userContextService.UserId));

        return result.HandleOkResult(this);
    }

    [HttpGet("{walletId:guid}/categories")]
    public async Task<ActionResult<IList<CategoryDTO>>> GetCategoriesForWallet([FromRoute] Guid walletId)
    {
        var result =
            await mediator.Send(new GetActiveCategoriesForWalletQuery(
                UserId: userContextService.UserId,
                WalletId: walletId));

        return result.HandleOkResult(this);
    }

    [HttpGet("{walletId:guid}/tags")]
    public async Task<ActionResult<IList<TagDTO>>> GetTagsForWallet([FromRoute] Guid walletId)
    {
        var result =
            await mediator.Send(new GetActiveTagsForWalletQuery(
                UserId: userContextService.UserId,
                WalletId: walletId));

        return result.HandleOkResult(this);
    }
}