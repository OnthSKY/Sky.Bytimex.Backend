namespace Sky.Template.Backend.Core.Exceptions;

public class NotFoundException : Exception
{
    public string ResourceKey { get; }
    public object[] FormatArgs { get; }

    public NotFoundException(string resourceKey, params object[] formatArgs)
        : base(resourceKey)
    {
        ResourceKey = resourceKey;
        FormatArgs = formatArgs;
    }
}
