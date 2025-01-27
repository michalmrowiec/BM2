namespace BM2.Application.Responses
{
    public class BaseResponse
    {
        public ResponseStatus Status { get; set; }
        public string? Message { get; set; }
        public List<string>? ValidationErrors { get; set; }

        public BaseResponse()
        {
            Status = ResponseStatus.Success;
            ValidationErrors = new();
        }

        public BaseResponse(ResponseStatus status, string message)
        {
            Status = status;
            Message = message;
            ValidationErrors = new();
        }

        public BaseResponse(FluentValidation.Results.ValidationResult validationResult)
        {
            Status = ResponseStatus.ValidationError;
            ValidationErrors = new();
            validationResult.Errors
                .ForEach(e => ValidationErrors.Add(e.ErrorMessage));
        }

        public BaseResponse(ResponseStatus status, string? message, FluentValidation.Results.ValidationResult validationResult)
        {
            Status = status;
            Message = message;
            ValidationErrors = new();
            validationResult.Errors
                .ForEach(e => ValidationErrors.Add(e.ErrorMessage));
        }

        public enum ResponseStatus
        {
            Success = 0,
            NotFound = 1,
            BadQuery = 2,
            ServerError = 3,
            ValidationError = 4
        }

        public bool IsSuccess() => Status == ResponseStatus.Success;
    }

    public class BaseResponse<T> : BaseResponse where T : class
    {
        public T? ReturnedObj { get; set; }

        public BaseResponse(T obj)
        {
            Status = ResponseStatus.Success;
            ValidationErrors = [];
            ReturnedObj = obj;
        }

        public BaseResponse(FluentValidation.Results.ValidationResult validationResult) : base(validationResult) { }

        public BaseResponse(ResponseStatus status, string message) : base(status, message) { }
    }
}
