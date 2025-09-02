namespace Sky.Template.Backend.Core.Exceptions;

public class AuthorizationException : Exception
{
    public AuthorizationException(string messageKey) : base(messageKey) { }
}
