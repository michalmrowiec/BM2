using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.RecordStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class RecordStatusesController(
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

    [HttpGet("for-periodic-records")]
    public async Task<ActionResult<IEnumerable<RecordStatusDTO>>> GetStatusesForPeriodicRecords()
    {
        var result = await mediator.Send(new GetStatusesForPeriodicRecordsQuery());

        return result.HandleOkResult(this);
    }
}
