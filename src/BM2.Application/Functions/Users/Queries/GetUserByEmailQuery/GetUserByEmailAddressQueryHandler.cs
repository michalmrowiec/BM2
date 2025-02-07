using AutoMapper;
using BM2.Application.Contracts.Persistence;
using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Users.Queries.GetUserByEmailQuery;

public class GetUserByEmailAddressQueryHandler(IUserRepository userRepository, IMapper mapper)
    : IRequestHandler<GetUserByEmailAddressQuery, BaseResponse<UserDTO>>
{
    public async Task<BaseResponse<UserDTO>> Handle(GetUserByEmailAddressQuery request,
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
            return new BaseResponse<UserDTO>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");


        UserDTO userDto;
        try
        {
            userDto = mapper.Map<UserDTO>(user);
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return new BaseResponse<UserDTO>(userDto);
    }
}