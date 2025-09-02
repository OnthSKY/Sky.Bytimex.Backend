using Microsoft.Extensions.Caching.Memory;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Infrastructure.Caching;

public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new();

    public MemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string?> GetAsync(string key)
    {
        _cache.TryGetValue(key, out string? value);
        return Task.FromResult(value);
    }

    public Task SetAsync(string key, string value, TimeSpan duration)
    {
        _cache.Set(key, value, duration);
        lock (_keys)
        {
            _keys.Add(key);
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        lock (_keys)
        {
            _keys.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
    {
        IEnumerable<string> result;
        lock (_keys)
        {
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            result = _keys.Where(k => System.Text.RegularExpressions.Regex.IsMatch(k, regexPattern)).ToList();
        }
        return Task.FromResult(result);
    }
}
