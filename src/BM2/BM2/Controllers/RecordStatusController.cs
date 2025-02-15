using BM2.Application.DTOs;
using BM2.Application.Functions.Curencies.Queries.Requests;
using BM2.Application.Functions.RecordStatuses.Queries.Requests;
using BM2.Controllers.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class RecordStatusController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [HttpGet("for-records")]
    public async Task<ActionResult<IEnumerable<RecordStatusDTO>>> GetStatusesForRecords()
    {
        var result = await mediator.Send(new GetStatusesForRecordsQuery());

        return result.HandleOkResult(this);
    }
}