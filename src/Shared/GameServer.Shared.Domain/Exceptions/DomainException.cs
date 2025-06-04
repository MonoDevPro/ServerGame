namespace GameServer.Shared.Domain.Exceptions;

public class DomainException : Exception
{
    private readonly Exception? _innerException;
    public DomainException(string message) : base(message)
    {
    }
    
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
        _innerException = innerException;
    }

    public override Exception GetBaseException()
    {
        return _innerException?.GetBaseException() ?? this;
    }
}