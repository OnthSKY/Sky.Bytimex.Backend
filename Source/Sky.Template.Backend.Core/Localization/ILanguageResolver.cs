namespace Sky.Template.Backend.Core.Localization
{
    public interface ILanguageResolver
    {
        /// <summary>Returns a normalized 2-letter language code (e.g., "tr", "en").</summary>
        string GetLanguageOrDefault();
    }
}
