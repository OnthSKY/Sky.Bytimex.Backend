using System.Reflection;
using System.Text;
using System.Text.Json;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Infrastructure.Caching;

public class CacheKeyGenerator : ICacheKeyGenerator
{
    public string GenerateCacheKey(MethodInfo methodInfo, object[] args)
    {
        var sb = new StringBuilder($"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}");
        foreach (var arg in args)
        {
            sb.Append(':');
            sb.Append(JsonSerializer.Serialize(arg));
        }
        return sb.ToString();
    }
}
