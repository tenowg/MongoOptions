using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoOptions.Data;

namespace MongoOptions.Interfaces
{
    public interface IMongoConnection<T> where T : class, new()
    {
        IMongoCollection<ConfigDocument<T>>? Collection { get; set; }
        string Database { get; set; }
        string CollectionName { get; set; }
        MemoryCacheEntryOptions memoryOptions { get; set; }
        MongoConfigurationOptions MongoConfigs { get; set; }
        IOptionsMonitorCache<T> OptionsCache { get; set; }
        IOptionsChangeTokenSource<T> OptionsChange { get; set; }

        void OnChanged(string? name = null);
    }

    public interface IMongoConnection
    {
        string Database { get; set; }
        string CollectionName { get; set; }
        Type Type { get; }
        void OnChanged(string? name = null);
        
    }
}