using System.Reflection;

namespace Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

public interface ICacheKeyGenerator
{
    string GenerateCacheKey(MethodInfo methodInfo, object[] args);
}
