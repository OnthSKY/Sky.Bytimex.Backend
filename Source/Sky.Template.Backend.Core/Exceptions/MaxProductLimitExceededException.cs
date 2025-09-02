namespace Sky.Template.Backend.Core.Exceptions;

public class MaxProductLimitExceededException : Exception
{
    public MaxProductLimitExceededException() : base("MaxProductLimitExceeded")
    {
    }
}

