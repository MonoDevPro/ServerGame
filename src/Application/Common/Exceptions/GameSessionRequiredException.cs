namespace GameServer.Application.Common.Exceptions;

public class GameSessionRequiredException : Exception
{
    public GameSessionRequiredException() : base("Game session is required") { }
    public GameSessionRequiredException(string message) : base(message) { }
    public GameSessionRequiredException(string message, Exception innerException) : base(message, innerException) { }
}
