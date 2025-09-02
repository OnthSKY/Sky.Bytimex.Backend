namespace Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

public interface ICacheService
{
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions options);
    Task RemoveAsync(string key);
    Task<IEnumerable<string>> SearchKeysAsync(string pattern);
}
