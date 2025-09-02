namespace Sky.Template.Backend.Core.Exceptions;

public class KycRequiredException : Exception
{
    public KycRequiredException() : base("KycRequired")
    {
    }
}

