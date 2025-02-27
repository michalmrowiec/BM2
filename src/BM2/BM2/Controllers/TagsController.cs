using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Tag;
using BM2.Shared.Requests.Queries.Tag;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TagsController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TagDTO>> AddTag([FromBody] AddTagCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }

    [HttpPost("wallet-relations")]
    public async Task<ActionResult<IList<TagWalletRelationDTO>>> SetTagWalletRelations(
        [FromBody] SetWalletTagRelationsCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpGet("wallet-relations")]
    public async Task<ActionResult<IList<TagWalletRelationDTO>>> GetAllTagsWithWalletRelations()
    {
        var result =
            await mediator.Send(new GetAllTagsForUserWithWalletRelationsQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }
}