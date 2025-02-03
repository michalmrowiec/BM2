using AutoMapper;
using BM2.Application.Contracts.Persistence;
using BM2.Application.Functions.DTOs;
using BM2.Application.Functions.Requests.Query;
using BM2.Application.Responses;
using BM2.Domain.Entities;
using MediatR;

namespace BM2.Application.Functions.Handlers.Query;

public class GetUserByEmailAddressQueryHandler(IUserRepository userRepository, IMapper mapper)
    : IRequestHandler<GetUserByEmailAddressQuery, BaseResponse<UserDto>>
{
    public async Task<BaseResponse<UserDto>> Handle(GetUserByEmailAddressQuery request,
        CancellationToken cancellationToken)
    {
        //User user;
        // try
        // {
        //     user = await userRepository.GetByEmailAddressAsync(request.EmailAddress);
        // }
        // catch (KeyNotFoundException ex)
        // {
        //     return new BaseResponse<UserDto>
        //         (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");
        // }
        // catch (Exception ex)
        // {
        //     return request.ReturnServerError();
        // }

        var user = await userRepository.GetByEmailAddressAsync(request.EmailAddress);

        if (user == null)
            return new BaseResponse<UserDto>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");


        UserDto userDto;
        try
        {
            userDto = mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return new BaseResponse<UserDto>(userDto);
    }
}