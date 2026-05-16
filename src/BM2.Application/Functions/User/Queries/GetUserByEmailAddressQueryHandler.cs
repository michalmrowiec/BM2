using BM2.Application.Mappings;
using BM2.Infrastructure.Repositories.Base;
using BM2.Application.Responses;
using BM2.Shared.DTOs;
using BM2.Shared.Requests.Queries.User;
using MediatR;

namespace BM2.Application.Functions.User.Queries;

public class GetUserByEmailAddressQueryHandler(UnitOfWork unitOfWork)
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

        var user = await unitOfWork.UserRepository.GetByEmailAddressAsync(request.EmailAddress);

        if (user == null)
            return new BaseResponse<UserDTO>
                (BaseResponse.ResponseStatus.BadQuery, "Login or password are wrong.");


        UserDTO userDto;
        try
        {
            userDto = user.ToDto();
        }
        catch (Exception ex)
        {
            return request.ReturnServerError();
        }

        return new BaseResponse<UserDTO>(userDto);
    }
}
