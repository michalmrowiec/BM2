using AutoMapper;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.DTOs;
using BM2.Application.Functions.User.Commands.Requests;
using BM2.Application.Functions.User.Commands.Validators;
using BM2.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.User.Commands.Handlers;

internal class AddUserCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediator mediator,
    IPasswordHasher<Domain.Entities.UserProfile.User> passwordHasher)
    : IRequestHandler<AddUserCommand, BaseResponse<UserDTO>>
{
    public async Task<BaseResponse<UserDTO>> Handle
        (AddUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult =
            await new AddUserValidator(mediator).ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid) return new BaseResponse<UserDTO>(validationResult);

        var newUser = mapper.Map<Domain.Entities.UserProfile.User>(request);
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
        newUser.IsActive = true;
        newUser.Id = Guid.NewGuid();
        newUser.CreatedBy = newUser.Id;
        newUser.CreatedAt = DateTime.UtcNow;

        UserDTO userDto;
        try
        {
            var createdUser = await unitOfWork.UserRepository.Add(newUser);
            await unitOfWork.SaveAsync();

            userDto = mapper.Map<UserDTO>(createdUser);
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return request.ReturnSuccessWithObject(userDto);
    }
}