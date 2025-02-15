using BM2.Application.DTOs;
using BM2.Application.Functions.Currency.Queries.Requests;
using BM2.Controllers.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class CurrencyController(
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