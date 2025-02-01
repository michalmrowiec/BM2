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
        User user;
        try
        {
            user = await userRepository.GetByEmailAddressAsync(request.EmailAddress);
        }
        catch (KeyNotFoundException ex)
        {
            return new BaseResponse<LoggedUserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return new BaseResponse<LoggedUserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");

        var jwtToken = jwtTokenService.GenerateJwt(user);

        LoggedUserDto loggedEmployee = new(user.EmailAddress, jwtToken);

        try
        {
            await auditLoginRepository.AddAsync(AuditLogin.CreateInstance(user.Id));
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return request.ReturnSuccessWithObject(loggedEmployee);
    }
}