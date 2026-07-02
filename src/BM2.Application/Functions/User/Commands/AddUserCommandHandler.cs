using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Functions.User.Commands.Validators;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Commands.User;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.User.Commands;

internal class AddUserCommandHandler(
    UnitOfWork unitOfWork,
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

        var newUser = request.ToEntity();
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

            userDto = createdUser.ToDto();
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return request.ReturnSuccessWithObject(userDto);
    }
}
