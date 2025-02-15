namespace BM2.Domain.Exceptions;

public static class DomainExceptions
{
    public class NotFoundException(string? message = null, Exception? innerException = null)
        : Exception(message, innerException);

    public class UnauthenticatedException(string? message = null, Exception? innerException = null)
        : Exception(message, innerException);
}