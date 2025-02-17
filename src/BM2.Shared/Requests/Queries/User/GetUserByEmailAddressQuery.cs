using BM2.Application.Responses;
using BM2.Shared.DTOs;
using MediatR;

namespace BM2.Shared.Requests.Queries.User;

public record GetUserByEmailAddressQuery(string EmailAddress) : IBaseRequest<UserDTO>;