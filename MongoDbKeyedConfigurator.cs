using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoOptions.Attributes;
using MongoOptions.Data;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MongoOptions
{
    /// <summary>
    /// Represents a configuration document stored in MongoDB.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    internal class ConfigDocument<T>
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
    public class MongoDbKeyedConfigurator<T> : IConfigureNamedOptions<T> where T : class, new()
    {
        private readonly IMemoryCache _cache;
        private readonly IMongoCollection<ConfigDocument<T>> _collection;
        private readonly MongoConfigurationOptions _configuration;

        /// <summary>
        /// Initializes a new instance of the MongoDbKeyedConfigurator.
        /// </summary>
        /// <param name="cache">The memory cache for storing loaded configurations.</param>
        /// <param name="client">The MongoDB client.</param>
        /// <param name="options">The MongoDB configuration options.</param>
        public MongoDbKeyedConfigurator(IMemoryCache cache, IMongoClient client, MongoConfigurationOptions options)
        {
            _cache = cache;
            _configuration = options;

            var optionsAttr = typeof(T).GetCustomAttribute<OptionsAttribute>();

            var collection = optionsAttr?.CollectionName ?? typeof(T).Name;
            var database = optionsAttr?.DatabaseName ?? options.DatabaseName;

            _collection = client.GetDatabase(database).GetCollection<ConfigDocument<T>>(collection);
        }

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
                        ? "Default"
                        : name;

            string cacheKey = $"{_configuration.CachePrefix}{typeof(T).Name}_{lookupName}";

            if (!_cache.TryGetValue(cacheKey, out ConfigDocument<T>? cachedSettings) || cachedSettings!.IsExpired)
            {
                try
                {
                    var filter = Builders<ConfigDocument<T>>.Filter.Eq("Name", lookupName);
                    var freshResult = _collection.Find(filter).FirstOrDefault();

                    if (freshResult != null)
                    {
                        cachedSettings = freshResult;
                        cachedSettings.ExpiresAt = DateTime.UtcNow.Add(_configuration.CacheSoftDuration);
                        _cache.Set(cacheKey, cachedSettings, _configuration.CacheHardDuration);
                    }
                }
                catch
                {
                    if (cachedSettings != null)
                    {
                        cachedSettings.ExpiresAt = DateTime.UtcNow.AddMinutes(1);
                        _cache.Set(cacheKey, cachedSettings, _configuration.CacheHardDuration);
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
                foreach (var prop in typeof(T).GetProperties().Where(p => p.CanWrite))
                {
                    prop.SetValue(options, prop.GetValue(cachedSettings.Value));
                }

                var context = new ValidationContext(options);
                var results = new List<ValidationResult>();

                if (!Validator.TryValidateObject(options, context, results, true))
                {
                    var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
                    // You can log this or throw an exception to prevent bad data from leaking in
                    throw new OptionsValidationException(name ?? "Default", typeof(T), [errors]);
                }
            }
        }
    }
}