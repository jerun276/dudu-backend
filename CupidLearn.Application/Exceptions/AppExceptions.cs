namespace CupidLearn.Application.Exceptions;

public abstract class AppException(string message) : Exception(message)
{
    public abstract int StatusCode { get; }

    public abstract string Title { get; }
}

public sealed class BadRequestException(string message) : AppException(message)
{
    public override int StatusCode => 400;

    public override string Title => "Bad Request";
}

public sealed class UnauthorizedException(string message) : AppException(message)
{
    public override int StatusCode => 401;

    public override string Title => "Unauthorized";
}

public sealed class ForbiddenException(string message) : AppException(message)
{
    public override int StatusCode => 403;

    public override string Title => "Forbidden";
}

public sealed class NotFoundException(string message) : AppException(message)
{
    public override int StatusCode => 404;

    public override string Title => "Not Found";
}

public sealed class ConflictException(string message) : AppException(message)
{
    public override int StatusCode => 409;

    public override string Title => "Conflict";
}

public sealed class ServiceUnavailableException(string message) : AppException(message)
{
    public override int StatusCode => 503;

    public override string Title => "Service Unavailable";
}
