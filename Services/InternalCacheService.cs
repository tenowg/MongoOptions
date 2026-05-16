using Microsoft.Extensions.Caching.Memory;
using MongoOptions.Data;

namespace MongoOptions.Services
{
    internal class InternalCacheService(IMemoryCache cache, MongoConfigurationOptions options)
    {
        public void Remove<T>(string key)
        {
            cache.Remove(BuildKey<T>(key));
        }

        public static void Clear() { }

        public ConfigDocument<T> Add<T>(string key, ConfigDocument<T> value, MemoryCacheEntryOptions options) 
        {
            return cache.Set(BuildKey<T>(key), value, options);
        }

        public bool TryGet<T>(string key, out ConfigDocument<T>? doc)
        {
            return cache.TryGetValue(BuildKey<T>(key), out doc);
        }

        private string BuildKey<T>(string key)
        {
            return $"{options.CachePrefix}{typeof(T).Name}_{key}";
        }
    }
}
