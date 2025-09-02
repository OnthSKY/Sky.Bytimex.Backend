using Microsoft.Extensions.Localization;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Core.Exceptions;

public class BusinessRulesException : Exception
{
    public string ResourceKey { get; }
    public object[] FormatArgs { get; }

    private static string GetLocalizedMessage(string resourceKey, object[] formatArgs)
    {
        var provider = ServiceTool.ServiceProvider;
        if (provider == null)
            return resourceKey;

        var localizer = provider.GetService(typeof(IStringLocalizer<SharedResource>)) as IStringLocalizer<SharedResource>;
        return localizer != null ? localizer[resourceKey, formatArgs] : resourceKey;
    }

    public BusinessRulesException(string resourceKey)
        : base(GetLocalizedMessage(resourceKey, Array.Empty<object>()))
    {
        ResourceKey = resourceKey;
        FormatArgs = Array.Empty<object>();
    }

    public BusinessRulesException(string resourceKey, params object[] formatArgs)
        : base(GetLocalizedMessage(resourceKey, formatArgs))
    {
        ResourceKey = resourceKey;
        FormatArgs = formatArgs;
    }
}
