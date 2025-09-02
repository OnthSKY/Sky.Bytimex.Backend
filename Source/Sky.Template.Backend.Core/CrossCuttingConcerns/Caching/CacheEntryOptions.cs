namespace Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

public class CacheEntryOptions
{
    public TimeSpan? SlidingExpiration { get; set; }
    public TimeSpan? AbsoluteExpiration { get; set; }
}
