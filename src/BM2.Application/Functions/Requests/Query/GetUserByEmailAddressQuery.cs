using BM2.Application.Functions.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Requests.Query;

public record GetUserByEmailAddressQuery(string EmailAddress) : IRequest<BaseResponse<UserDto>>;