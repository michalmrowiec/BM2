using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Responses;
using BM2.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.Users.Commands.AddUserCommand;

internal class AddUserCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediator mediator,
    IPasswordHasher<User> passwordHasher)
    : IRequestHandler<AddUserCommand, BaseResponse<UserDTO>>
{
    public async Task<BaseResponse<UserDTO>> Handle
        (AddUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddUserValidator(mediator).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<UserDTO>(validationResult);

        var newUser = mapper.Map<User>(request);
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
        newUser.IsActive = true;
        newUser.Id = Guid.NewGuid();
        newUser.CreatedBy = newUser.Id;
        newUser.CreatedAt = DateTime.UtcNow;

        UserDTO userDto;
        try
        {
            var createdUser = await unitOfWork.UserRepository.Add(newUser);
            await unitOfWork.UserRepository.Save();

            userDto = mapper.Map<UserDTO>(createdUser);
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return request.ReturnSuccessWithObject(userDto);
    }
}