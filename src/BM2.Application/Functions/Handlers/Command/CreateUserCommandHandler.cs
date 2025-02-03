using AutoMapper;
using BM2.Application.Contracts.Persistence;
using BM2.Application.Functions.DTOs;
using BM2.Application.Functions.Requests.Command;
using BM2.Application.Functions.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.Handlers.Command;

internal class CreateUserCommandHandler(
    IUserRepository userRepository,
    IMapper mapper,
    IMediator mediator,
    IPasswordHasher<User> passwordHasher)
    : IRequestHandler<CreateUserCommand, BaseResponse<UserDto>>
{
    public async Task<BaseResponse<UserDto>> Handle
        (CreateUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new CreateUserValidator(mediator).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<UserDto>(validationResult);

        var newUser = mapper.Map<User>(request);
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
        newUser.IsActive = true;
        newUser.Id = Guid.NewGuid();
        newUser.CreatedBy = newUser.Id;
        newUser.CreatedAt = DateTime.UtcNow;

        UserDto userDto;
        try
        {
            var createdUser = await userRepository.AddAsync(newUser);

            userDto = mapper.Map<UserDto>(createdUser);
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return request.ReturnSuccessWithObject(userDto);
    }
}