using BM2.Domain.Entities;
using BM2.Domain.Entities.Interfaces;
using BM2.Domain.Exceptions;
using MediatR;

namespace BM2.Application.Responses;

internal static class ResponseHelper
{
    internal static BaseResponse<T> ReturnServerError<T>(this IRequest<BaseResponse<T>> request) where T : class
    {
        return new BaseResponse<T>(BaseResponse.ResponseStatus.ServerError, "Something went wrong.");
    }

    internal static BaseResponse<T> ReturnSuccessWithObject<T>(this IRequest<BaseResponse<T>> request, T obj)
        where T : class
    {
        return new BaseResponse<T>(obj);
    }

    internal static BaseResponse<T> ReturnNotFound<T>(this IRequest<BaseResponse<T>> request) where T : class
    {
        return new BaseResponse<T>(BaseResponse.ResponseStatus.NotFound, "Not found.");
    }

    internal static void ThrowExceptionIfNull<T>(this T? obj) where T : class
    {
        if (obj is null)
            throw new DomainExceptions.NotFoundException($"{nameof(T)} not found.");
    }

    internal static void CheckPermission<T>(this T obj, Guid userId) where T : IOwnedByUser
    {
        if (obj.OwnedByUserId != userId)
            throw new UnauthorizedAccessException();
    }

    internal static void CheckPermission<T>(this IEnumerable<T> objs, Guid userId) where T : IOwnedByUser
    {
        var ownedByUsers = objs.ToList();
        if(!ownedByUsers.Any())
            return;
        
        if (ownedByUsers.Any(x => x.OwnedByUserId != userId))
            throw new UnauthorizedAccessException();
    }
}