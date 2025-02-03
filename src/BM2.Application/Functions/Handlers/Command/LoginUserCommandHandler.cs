using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Services;
using BM2.Application.Functions.DTOs;
using BM2.Application.Functions.Requests.Command;
using BM2.Application.Responses;
using BM2.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.Handlers.Command;

public class LoginUserCommandHandler(
    IUserRepository userRepository,
    IAuditLoginRepository auditLoginRepository,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginUserCommand, BaseResponse<LoggedUserDto>>
{
    public async Task<BaseResponse<LoggedUserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAddressAsync(request.EmailAddress);

        if (user == null)
            return new BaseResponse<LoggedUserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return new BaseResponse<LoggedUserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        if (user.DeletedAt != null)
            return new BaseResponse<LoggedUserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        if (!user.IsActive)
            return new BaseResponse<LoggedUserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Account is blocked. Contact to administrator.");

        var jwtToken = jwtTokenService.GenerateJwt(user);

        LoggedUserDto loggedEmployee = new(user.EmailAddress, jwtToken);

        await auditLoginRepository.AddAsync(AuditLogin.CreateInstance(user.Id));

        return request.ReturnSuccessWithObject(loggedEmployee);
    }
}