using BM2.Application.DTOs;
using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Users.Queries.GetUserByEmailQuery;

public record GetUserByEmailAddressQuery(string EmailAddress) : IRequest<BaseResponse<UserDTO>>;