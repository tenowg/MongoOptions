using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoOptions.Attributes;
using MongoOptions.Interfaces;
using MongoOptions.Services;
using MongoOptions.Types;
using System.Reflection;
using System.Xml.Linq;

namespace MongoOptions.Data
{
    public partial class MongoConnection<T> : IMongoConnection, IMongoConnection<T> where T : class, IConfigFile, new()
    {
        public string Database { get; set; }
        public string CollectionName { get; set; }
        public IMongoCollection<ConfigDocument<T>>? Collection { get; set; }
        public IOptionsMonitorCache<T> OptionsCache { get; set; }
        public IOptionsChangeTokenSource<T> OptionsChange { get; set; }
        public MemoryCacheEntryOptions memoryOptions { get; set; }
        public MongoConfigurationOptions MongoConfigs { get; set; }
        public Type Type { get; set; } = typeof(T);
        public IConfigFile Instance { get; set; }
        public InternalCacheService Cache { get; set; }

        private IServiceProvider sp { get; set; }

        public MongoConnection(IServiceProvider sp, IMongoClient client, IOptionsMonitorCache<T> optionsCache, IOptionsChangeTokenSource<T> optionsChange, InternalCacheService cache, MongoConfigurationOptions options)
        {
            OptionsCache = optionsCache;
            OptionsChange = optionsChange;
            MongoConfigs = options;
            Instance = new T();
            this.sp = sp;
            Cache = cache;

            var optionsAttr = typeof(T).GetCustomAttribute<MongoOptionAttribute>();
            CollectionName = optionsAttr?.CollectionName ?? typeof(T).Name;
            Database = optionsAttr?.DatabaseName ?? options.DatabaseName;

            Collection = client.GetDatabase(Database).GetCollection<ConfigDocument<T>>(CollectionName);

            memoryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = options.CacheHardDuration
            };
        }

        public IOptionsMonitor<T> GetMonitor()
        {
            return sp.GetService<IOptionsMonitor<T>>();
        }

        public void OnStarted(string? name = null)
        {
            if (name == null || name == MongoDefaultOptions.DefaultName)
            {
                name = Options.DefaultName;
            }

            //((MongoChangeTokenSource<T>)OptionsChange).OnMongoChanged(name);
            OptionsCache.TryRemove(name);
        }

        public void OnChanged(string? name = null)
        {
            if (name == null || name == MongoDefaultOptions.DefaultName)
            {
                name = Options.DefaultName;
            }
            Cache.Remove<T>(name);
            ((MongoChangeTokenSource<T>)OptionsChange).OnMongoChanged(name);
            OptionsCache.TryRemove(name);
        }

        public void OnChanged(BsonDocument fullDocument)
        {
            var updatedConfig = BsonSerializer.Deserialize<ConfigDocument<T>>(fullDocument);

            OnChanged(updatedConfig.Name);
        }
    }
}
