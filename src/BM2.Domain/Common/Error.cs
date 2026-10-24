namespace BM2.Domain.Common;

public enum ErrorKind
{
    Validation,
    NotFound,
    Forbidden,
    Conflict,
    Gone,
}

/// <summary>A business error; <see cref="Field"/> names the request property it relates to.</summary>
public sealed record Error(ErrorKind Kind, string Message, string? Field = null)
{
    public static Error Validation(string field, string message) => new(ErrorKind.Validation, message, field);

    public static Error NotFound() => new(ErrorKind.NotFound, "Not found.");

    public static Error Forbidden() => new(ErrorKind.Forbidden, "You don't have permission to do this.");

    public static Error Conflict(string message, string? field = null) => new(ErrorKind.Conflict, message, field);

    public static Error Gone(string message) => new(ErrorKind.Gone, message);
}
