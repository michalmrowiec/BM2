using BM2.Application.Functions.DTOs;
using BM2.Application.Functions.Requests.Command;
using BM2.Controllers.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;
        private readonly IUserContextService _userContextService;

        public UserController(
            ILogger<UserController> logger,
            IMediator mediator,
            IUserContextService userContextService)
        {
            _mediator = mediator;
            _logger = logger;
            _userContextService = userContextService;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateEmployee
            ([FromBody] CreateUserCommand createUserCommand)
        {
            var result = await _mediator.Send(createUserCommand);

            return result.HandleCreatedResult(this, "");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoggedUserDto>> Login([FromBody] LoginUserCommand loginUserCommand)
        {
            var result = await _mediator.Send(loginUserCommand);

            return result.HandleOkResult(this);
        }
    }
}