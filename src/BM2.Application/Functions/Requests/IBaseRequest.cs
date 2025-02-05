using BM2.Application.Responses;
using MediatR;

namespace BM2.Application.Functions.Requests;

public interface IBaseRequest<T> : IRequest<BaseResponse<T>> where T : class;