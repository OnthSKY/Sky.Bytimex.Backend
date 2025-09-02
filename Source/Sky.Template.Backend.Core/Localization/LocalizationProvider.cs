

using Microsoft.Extensions.Localization;

namespace Sky.Template.Backend.Core.Localization;

public static class LocalizationProvider
{
    public static IStringLocalizer<SharedResource>? Localizer { get; set; }
}
