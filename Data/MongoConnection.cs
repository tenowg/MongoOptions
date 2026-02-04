using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoOptions.Attributes;
using MongoOptions.Interfaces;
using System.Reflection;

namespace MongoOptions.Data
{
    public partial class MongoConnection<T> : IMongoConnection, IMongoConnection<T> where T : class, new()
    {
        public string Database { get; set; }
        public string CollectionName { get; set; }
        public IMongoCollection<ConfigDocument<T>>? Collection { get; set; }
        public IOptionsMonitorCache<T> OptionsCache { get; set; }
        public IOptionsChangeTokenSource<T> OptionsChange { get; set; }
        public MemoryCacheEntryOptions memoryOptions { get; set; }
        public MongoConfigurationOptions MongoConfigs { get; set; }
        public Type Type { get; set; } = typeof(T);

        public MongoConnection(IServiceProvider sp, IOptionsMonitorCache<T> optionsCache, IOptionsChangeTokenSource<T> optionsChange, MongoConfigurationOptions options)
        {
            OptionsCache = optionsCache;
            OptionsChange = optionsChange;
            MongoConfigs = options;

            var client = sp.GetRequiredService<IMongoClient>();
            var optionsAttr = typeof(T).GetCustomAttribute<MongoOptionAttribute>();
            CollectionName = optionsAttr?.CollectionName ?? typeof(T).Name;
            Database = optionsAttr?.DatabaseName ?? options.DatabaseName;

            Collection = client.GetDatabase(Database).GetCollection<ConfigDocument<T>>(CollectionName);

            memoryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = options.CacheHardDuration
            };
        }

        public void OnChanged(string? name = null)
        {
            if (name == null || name == Options.DefaultName)
            {
                name = MongoDefaultOptions.DefaultName;
            }

            ((MongoChangeTokenSource<T>)OptionsChange).OnMongoChanged(name);
            OptionsCache.TryRemove(name);
        }
    }
}
