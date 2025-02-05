using BM2.Application.Functions.DTOs;
using BM2.Application.Functions.Requests.Command;
using BM2.Controllers.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController(
        IMediator mediator,
        IUserContextService userContextService)
        : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> CreateEmployee
            ([FromBody] CreateUserCommand createUserCommand)
        {
            var result = await mediator.Send(createUserCommand);

            return result.HandleCreatedResult(this, "");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoggedUserDTO>> Login([FromBody] LoginUserCommand loginUserCommand)
        {
            var result = await mediator.Send(loginUserCommand);

            return result.HandleOkResult(this);
        }
    }
}