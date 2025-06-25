namespace GameServer.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Access forbidden") { }
    public ForbiddenException(string message) : base(message) { }
    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}
