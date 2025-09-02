namespace Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

public interface ICacheProvider
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan duration);
    Task RemoveAsync(string key);
    Task<IEnumerable<string>> SearchKeysAsync(string pattern);
}
