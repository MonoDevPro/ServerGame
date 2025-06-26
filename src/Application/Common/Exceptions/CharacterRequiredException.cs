namespace GameServer.Application.Common.Exceptions;

public class CharacterRequiredException : Exception
{
    public CharacterRequiredException(string message) : base(message) { }

    public CharacterRequiredException(string message, Exception innerException) : base(message, innerException) { }
}
