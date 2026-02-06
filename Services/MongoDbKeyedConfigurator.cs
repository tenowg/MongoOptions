using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Interfaces;
using MongoOptions.Types;
using System.ComponentModel.DataAnnotations;

namespace MongoOptions.Services
{
    /// <summary>
    /// Represents a configuration document stored in MongoDB.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    public class ConfigDocument<T>
    {
        /// <summary>
        /// The MongoDB ObjectId for the document.
        /// </summary>
        [BsonId]
        [BsonIgnoreIfDefault]
        public ObjectId? Id { get; set; }

        /// <summary>
        /// The name of the configuration instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The configuration value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The expiration time for caching purposes.
        /// </summary>
        [BsonIgnore]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

        /// <summary>
        /// Indicates whether the cached item has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Configures named options by loading them from MongoDB with caching and validation.
    /// Implements IConfigureNamedOptions to integrate with the .NET options pattern.
    /// </summary>
    /// <typeparam name="T">The type of options to configure.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the MongoDbKeyedConfigurator.
    /// </remarks>
    /// <param name="cache">The memory cache for storing loaded configurations.</param>
    /// <param name="collection">The MongoDB client.</param>
    /// <param name="options">The MongoDB configuration options.</param>
    public class MongoDbKeyedConfigurator<T>(IMemoryCache cache, IMongoConnection<T> mongoConnection) : IConfigureNamedOptions<T> where T : class, IConfigFile
    {
        private readonly IMemoryCache _cache = cache;
        private readonly IMongoCollection<ConfigDocument<T>> _collection = mongoConnection.Collection!;
        private readonly MongoConfigurationOptions _configuration = mongoConnection.MongoConfigs;
        private readonly IOptionsMonitorCache<T> optionsMonitor = mongoConnection.OptionsCache;

        /// <summary>
        /// Configures the default options instance.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        public void Configure(T options) => Configure(Options.DefaultName, options);

        /// <summary>
        /// Configures a named options instance by loading from MongoDB or cache.
        /// Validates the loaded options and handles caching with stale-on-failure.
        /// </summary>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="options">The options instance to configure.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the named configuration is not found.</exception>
        /// <exception cref="OptionsValidationException">Thrown if the loaded options fail validation.</exception>
        public void Configure(string? name, T options)
        {
            string lookupName = string.IsNullOrWhiteSpace(name) || name == Options.DefaultName
                        ? MongoDefaultOptions.DefaultName
                        : name;

            string cacheKey = $"{mongoConnection.MongoConfigs.CachePrefix}{typeof(T).Name}_{lookupName}";

            if (!_cache.TryGetValue(cacheKey, out ConfigDocument<T>? cachedSettings) || cachedSettings!.IsExpired)
            {
                try
                {
                    var filter = Builders<ConfigDocument<T>>.Filter.Eq(o => o.Name, lookupName);
                    var freshResult = _collection.Find(filter).FirstOrDefault();

                    if (freshResult != null)
                    {
                        cachedSettings = freshResult;
                        cachedSettings.ExpiresAt = DateTime.UtcNow.Add(_configuration.CacheSoftDuration);
                        _cache.Set(cacheKey, cachedSettings, mongoConnection.memoryOptions.RegisterPostEvictionCallback(CacheEvictionCallback, state: this));
                        if (name == MongoDefaultOptions.DefaultName || string.IsNullOrEmpty(name))
                        {
                            mongoConnection.OnChanged(Options.DefaultName);
                        } else
                        {
                            mongoConnection.OnChanged(name);
                        }                    
                    }
                }
                catch
                {
                    if (cachedSettings != null)
                    {
                        cachedSettings.ExpiresAt = DateTime.UtcNow.AddMinutes(1);
                        _cache.Set(cacheKey, cachedSettings, mongoConnection.memoryOptions.RegisterPostEvictionCallback(CacheEvictionCallback, state: this));
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Config with Name ({lookupName}) was not found.");
                    }
                }
            }

            if (cachedSettings != null)
            {
                // Copy properties from cached object to the 'options' instance
                foreach (var prop in cachedSettings.Value.GetProperties())
                {
                    prop.Setter(options, prop.Getter(cachedSettings.Value));
                }
            }
        }
        private void CacheEvictionCallback(object key, object? value, EvictionReason reason, object? state)
        {
            var cachedValue = (ConfigDocument<T>?)value;
            var cachedKey = cachedValue?.Name ?? MongoDefaultOptions.DefaultName;
            if (cachedKey == MongoDefaultOptions.DefaultName)
            {
                cachedKey = Options.DefaultName;
            }
            optionsMonitor.TryRemove(cachedKey);
        }
    }
}