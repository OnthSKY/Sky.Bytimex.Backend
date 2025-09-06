using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Infrastructure.Localization
{
    public class WebLanguageResolver : ILanguageResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WebLanguageResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetLanguageOrDefault()
        {
            var al = _httpContextAccessor.HttpContext?.Request?.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(al)) return "en";

            var first = al.Split(',').FirstOrDefault()?.Trim().ToLower();
            if (string.IsNullOrEmpty(first)) return "en";
            if (first.Length > 2) first = first[..2];
            return first;
        }
    }
}
