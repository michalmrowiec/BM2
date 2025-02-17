using BM2.Application.Responses;
using MediatR;

namespace BM2.Shared.Requests;

public interface IBaseRequest<T> : IRequest<BaseResponse<T>> where T : class;

public interface IBaseRequestCollection<T> : IRequest<BaseResponse<IEnumerable<T>>> where T : class;