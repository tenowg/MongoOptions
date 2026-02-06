using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Services;

namespace MongoOptions.Interfaces
{
    public interface IMongoConnection<T> where T : class, IConfigFile
    {
        IMongoCollection<ConfigDocument<T>>? Collection { get; set; }
        string Database { get; set; }
        string CollectionName { get; set; }
        MemoryCacheEntryOptions memoryOptions { get; set; }
        MongoConfigurationOptions MongoConfigs { get; set; }
        IOptionsMonitorCache<T> OptionsCache { get; set; }
        IOptionsChangeTokenSource<T> OptionsChange { get; set; }

        IOptionsMonitor<T> GetMonitor();
        void OnChanged(string? name = null);
    }

    public interface IMongoConnection
    {
        IConfigFile Instance { get; set; }
        string Database { get; set; }
        string CollectionName { get; set; }
        Type Type { get; }
        void OnChanged(string? name = null);
        void OnChanged(BsonDocument fullDocument);
    }
}