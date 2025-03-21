﻿using BM2.Controllers.Utils;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(
    IMediator mediator,
    IUserContextService userContextService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> CreateEmployee([FromBody] AddUserCommand addUserCommand)
    {
        var result = await mediator.Send(addUserCommand);

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