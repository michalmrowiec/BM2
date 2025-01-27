using AutoMapper;
using BM2.Application.Contracts.Persistence;
using BM2.Application.Functions.Commands;
using BM2.Application.Functions.Dtos;
using BM2.Application.Functions.Validators;
using BM2.Application.Responses;
using BM2.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BM2.Application.Functions.Handlers
{
    internal class CreateUserCommandHandler(
        IUserRepository userRepository,
        IMapper mapper,
        IPasswordHasher<User> passwordHasher)
        : IRequestHandler<CreateUserCommand, BaseResponse<UserDto>>
    {
        public async Task<BaseResponse<UserDto>> Handle
            (CreateUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult =
                await new CreateUserValidator().ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                return new BaseResponse<UserDto>(validationResult);
            }

            var newUser = mapper.Map<User>(request);
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
            newUser.CreatedAt = DateTime.UtcNow;

            UserDto userDto;
            try
            {
                var createdUser = await userRepository.CreateAsync(newUser);

                userDto = mapper.Map<UserDto>(createdUser);
            }
            catch (Exception ex)
            {
                return new BaseResponse<UserDto>(BaseResponse.ResponseStatus.ServerError, "Something went wrong.");
            }

            return new BaseResponse<UserDto>(userDto);
        }
    }
}