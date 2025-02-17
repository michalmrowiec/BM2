using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.Currency;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class CurrenciesController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrencyDTO>>> GetAllCurrencies()
    {
        var result = await mediator.Send(new GetAllCurrenciesQuery());

        return result.HandleOkResult(this);
    }
}