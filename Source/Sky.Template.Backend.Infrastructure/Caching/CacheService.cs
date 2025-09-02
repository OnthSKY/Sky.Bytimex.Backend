using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using System.Net;
using System.Text.Json;

namespace Sky.Template.Backend.Infrastructure.Caching;

public class CacheService : ICacheService
{
    private readonly ICacheProvider _provider;

    public CacheService(ICacheProvider provider)
    {
        _provider = provider;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions options)
    {
        var cached = await _provider.GetAsync(key);
        if (cached is not null)
        {
            if (options.SlidingExpiration.HasValue)
            {
                await _provider.SetAsync(key, cached, options.SlidingExpiration.Value);
            }

            var deserialized = JsonSerializer.Deserialize<T>(cached)!;

            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseControllerResponse<>))
            {
                var statusCodeProp = type.GetProperty("StatusCode");
                if (statusCodeProp != null)
                {
                    var code = (HttpStatusCode)statusCodeProp.GetValue(deserialized)!;
                    if (code == 0)
                    {
                        statusCodeProp.SetValue(deserialized, HttpStatusCode.OK);
                    }
                }
            }

            return deserialized;
        }

        var value = await factory();
        var expiration = options.AbsoluteExpiration ?? options.SlidingExpiration ?? TimeSpan.FromMinutes(60);
        await _provider.SetAsync(key, JsonSerializer.Serialize(value), expiration);
        return value;
    }


    public Task RemoveAsync(string key) => _provider.RemoveAsync(key);

    public Task<IEnumerable<string>> SearchKeysAsync(string pattern) => _provider.SearchKeysAsync(pattern);
}
