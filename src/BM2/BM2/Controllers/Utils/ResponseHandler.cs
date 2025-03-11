using BM2.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BM2.Controllers.Utils;

public static class ResponseHandler
{
    public static ActionResult HandleOkResult<T>
        (this BaseResponse<T> result, ControllerBase controller)
        where T : class
    {
        return result.Status switch
        {
            BaseResponse.ResponseStatus.Success => controller.Ok(result.ReturnedObj),
            BaseResponse.ResponseStatus.ValidationError => controller.BadRequest(result.ValidationErrors),
            BaseResponse.ResponseStatus.ServerError => controller.StatusCode(500, result.Message),
            BaseResponse.ResponseStatus.NotFound => controller.NotFound(result.Message),
            BaseResponse.ResponseStatus.BadQuery => controller.BadRequest(result.Message),
            _ => controller.BadRequest()
        };
    }

    public static ActionResult HandleOkResult
        (this BaseResponse result, ControllerBase controller)
    {
        return result.Status switch
        {
            BaseResponse.ResponseStatus.Success => controller.Ok(),
            BaseResponse.ResponseStatus.ValidationError => controller.BadRequest(result.ValidationErrors),
            BaseResponse.ResponseStatus.ServerError => controller.StatusCode(500, result.Message),
            BaseResponse.ResponseStatus.NotFound => controller.NotFound(result.Message),
            BaseResponse.ResponseStatus.BadQuery => controller.BadRequest(result.Message),
            _ => controller.BadRequest()
        };
    }
    
    public static ActionResult HandleNoContentResult
        (this BaseResponse result, ControllerBase controller)
    {
        return result.Status switch
        {
            BaseResponse.ResponseStatus.Success => controller.NoContent(),
            BaseResponse.ResponseStatus.ValidationError => controller.BadRequest(result.ValidationErrors),
            BaseResponse.ResponseStatus.ServerError => controller.StatusCode(500, result.Message),
            BaseResponse.ResponseStatus.NotFound => controller.NotFound(result.Message),
            BaseResponse.ResponseStatus.BadQuery => controller.BadRequest(result.Message),
            _ => controller.BadRequest()
        };
    }

    public static ActionResult HandleCreatedResult<T>
        (this BaseResponse<T> result, ControllerBase controller, string location)
        where T : class
    {
        return result.Status switch
        {
            BaseResponse.ResponseStatus.Success => controller.Created(location, result.ReturnedObj),
            BaseResponse.ResponseStatus.ValidationError => controller.BadRequest(result.ValidationErrors),
            BaseResponse.ResponseStatus.ServerError => controller.StatusCode(500, result.Message),
            BaseResponse.ResponseStatus.NotFound => controller.NotFound(result.Message),
            BaseResponse.ResponseStatus.BadQuery => controller.BadRequest(result.Message),
            _ => controller.BadRequest()
        };
    }
}