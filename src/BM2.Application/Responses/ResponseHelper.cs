using MediatR;

namespace BM2.Application.Responses;

internal static class ResponseHelper
{
    internal static BaseResponse<T> ReturnServerError<T>(this IRequest<BaseResponse<T>> request) where T : class
    {
        return new BaseResponse<T>(BaseResponse.ResponseStatus.ServerError, "Something went wrong.");
    }
    
    internal static BaseResponse<T> ReturnSuccessWithObject<T>(this IRequest<BaseResponse<T>> request, T obj) where T : class
    {
        return new BaseResponse<T>(obj);
    }
}