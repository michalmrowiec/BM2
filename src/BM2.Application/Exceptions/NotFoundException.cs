namespace BM2.Application.Exceptions;

public class NotFoundException(string? message, Exception? innerException = null) : Exception(message, innerException);