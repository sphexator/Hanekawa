using Hanekawa.Interfaces;

namespace Hanekawa.Infrastructure.Caches;

public class CacheKeyProvider<T> where T : ICached
{
    public string GetKey(string key) => $"{typeof(T).Name}:{key}";
}