using BM2.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BM2.Server.Infrastructure;

public static class ProblemResults
{
    public static ProblemHttpResult ToProblem(this Error error) => error.Kind switch
    {
        ErrorKind.Validation => FieldProblem(error, StatusCodes.Status400BadRequest),
        ErrorKind.Conflict when error.Field is not null => FieldProblem(error, StatusCodes.Status409Conflict),
        ErrorKind.Conflict => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
        ErrorKind.NotFound => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
        ErrorKind.Forbidden => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden),
        ErrorKind.Gone => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status410Gone),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error.Kind, "Unknown error kind."),
    };

    // Same shape as ValidationProblem, so the client shows the message next to the field.
    private static ProblemHttpResult FieldProblem(Error error, int status) =>
        TypedResults.Problem(new HttpValidationProblemDetails(
            new Dictionary<string, string[]> { [error.Field ?? ""] = [error.Message] })
        {
            Status = status,
        });
}
