using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.Category;
using BM2.Shared.Requests.Queries.Category;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CategoryDTO>> AddCategory([FromBody] AddCategoryCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleCreatedResult(this, "");
    }

    [HttpPost("wallet-relations")]
    public async Task<ActionResult<IList<CategoryWalletRelationDTO>>> SetCategoryWalletRelations(
        [FromBody] SetWalletCategoryRelationsCommand command)
    {
        command.OwnedByUserId = userContextService.UserId;

        var result = await mediator.Send(command);

        return result.HandleOkResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<IList<CategoryDTO>>> GetAllCategories()
    {
        var result = await mediator.Send(new GetAllCategoriesForUserQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }

    [HttpGet("wallet-relations")]
    public async Task<ActionResult<IList<CategoryWalletRelationDTO>>> GetAllCategoriesWithWalletRelations()
    {
        var result =
            await mediator.Send(new GetAllCategoriesForUserWithWalletRelationsQuery(userContextService.UserId));

        return result.HandleOkResult(this);
    }
}