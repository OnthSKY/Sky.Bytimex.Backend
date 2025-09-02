using StackExchange.Redis;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Infrastructure.Caching;

public class RedisCacheProvider : ICacheProvider
{
    private readonly IDatabase _db;
    private readonly IServer _server;

    public RedisCacheProvider(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
        _server = multiplexer.GetServer(multiplexer.GetEndPoints()[0]);
    }

    public async Task<string?> GetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNull ? null : value.ToString();
    }

    public Task SetAsync(string key, string value, TimeSpan duration)
        => _db.StringSetAsync(key, value, duration);

    public Task RemoveAsync(string key)
        => _db.KeyDeleteAsync(key);

    public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
    {
        var keys = _server.Keys(pattern: pattern).Select(k => k.ToString());
        return Task.FromResult(keys);
    }
}
