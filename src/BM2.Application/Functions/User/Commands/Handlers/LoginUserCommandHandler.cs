using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Contracts.Services;
using BM2.Application.DTOs;
using BM2.Application.Functions.User.Commands.Requests;
using BM2.Application.Responses;
using BM2.Domain.Entities.UserProfile;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.User.Commands.Handlers;

public class LoginUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher<Domain.Entities.UserProfile.User> passwordHasher,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginUserCommand, BaseResponse<LoggedUserDTO>>
{
    public async Task<BaseResponse<LoggedUserDTO>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.UserRepository.GetByEmailAddressAsync(request.EmailAddress);

        if (user == null)
            return new BaseResponse<LoggedUserDTO>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return new BaseResponse<LoggedUserDTO>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        if (user.DeletedAt != null)
            return new BaseResponse<LoggedUserDTO>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        if (!user.IsActive)
            return new BaseResponse<LoggedUserDTO>
                (BaseResponse.ResponseStatus.BadQuery, "Account is blocked. Contact to administrator.");

        var jwtToken = jwtTokenService.GenerateJwt(user);

        LoggedUserDTO loggedEmployee = new(user.EmailAddress, jwtToken);

        await unitOfWork.AuditLoginRepository.Add(AuditLogin.CreateInstance(user.Id));
        await unitOfWork.AuditLoginRepository.Save();

        return request.ReturnSuccessWithObject(loggedEmployee);
    }
}