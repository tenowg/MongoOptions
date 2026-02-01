using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoOptions.Attributes;
using MongoOptions.Data;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MongoOptions
{
    /// <summary>
    /// Interface for managing configuration options stored in MongoDB.
    /// Provides methods for CRUD operations on configuration documents.
    /// </summary>
    public interface IConfigManager
    {
        /// <summary>
        /// Updates the default configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateConfigAsync<T>(T value) where T : class, new();

        /// <summary>
        /// Updates a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration instance.</param>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateConfigAsync<T>(string name, T value) where T : class, new();

        /// <summary>
        /// Retrieves all configuration keys for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <returns>A list of configuration keys.</returns>
        Task<List<string>> GetKeys<T>();

        /// <summary>
        /// Removes a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveConfig<T>(string name);

        /// <summary>
        /// Clones a configuration from one name to another.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="sourceName">The name of the source configuration.</param>
        /// <param name="targetName">The name of the target configuration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CloneConfigAsync<T>(string sourceName, string targetName) where T : class, new();
    }

    /// <summary>
    /// Implementation of IConfigManager for managing configuration options in MongoDB.
    /// Handles validation, caching, and database operations.
    /// </summary>
    public class MongoConfigManager(IMongoClient client, IMemoryCache cache, MongoConfigurationOptions configuration) : IConfigManager
    {
        /// <summary>
        /// Updates the default configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateConfigAsync<T>(T value) where T : class, new()
        {
            await UpdateConfigAsync("Default", value);
        }

        /// <summary>
        /// Updates a named configuration for the specified type.
        /// Validates the value before saving and clears the cache.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration instance.</param>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OptionsValidationException">Thrown if the value fails validation.</exception>
        public async Task UpdateConfigAsync<T>(string name, T value) where T : class, new()
        {
            var collection = GetCollection<T>();

            var context = new ValidationContext(value);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(value, context, results, true))
            {
                var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
                throw new OptionsValidationException(name ?? "Default", typeof(T), [errors]);
            }

            var document = new ConfigDocument<T> { Name = name, Value = value };
            await collection.ReplaceOneAsync(
                filter: d => d.Name == name,
                replacement: document,
                options: new ReplaceOptions { IsUpsert = true }
            );

            string cacheKey = $"{configuration.CachePrefix}{typeof(T).Name}_{name}";
            cache.Remove(cacheKey);
        }

        /// <summary>
        /// Retrieves all configuration keys for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <returns>A list of configuration keys.</returns>
        public async Task<List<string>> GetKeys<T>()
        {
            var collection = GetCollection<T>();

            return await collection.AsQueryable().Select(o => o.Name).ToListAsync();
        }

        /// <summary>
        /// Removes a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveConfig<T>(string name)
        {
            var collection = GetCollection<T>();

            await collection.DeleteOneAsync(o => o.Name == name);
        }

        /// <summary>
        /// Clones a configuration from one name to another.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="sourceName">The name of the source configuration.</param>
        /// <param name="targetName">The name of the target configuration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CloneConfigAsync<T>(string sourceName, string targetName) where T : class, new()
        {
            var collection = GetCollection<T>();

            var source = await collection.Find(d => d.Name == sourceName).FirstOrDefaultAsync();

            if (source != null)
            {
                await UpdateConfigAsync(targetName, source.Value);
            }
        }

        /// <summary>
        /// Gets the MongoDB collection for the specified type, using attributes for naming.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <returns>The MongoDB collection for the type.</returns>
        private IMongoCollection<ConfigDocument<T>> GetCollection<T>()
        {
            var optionsAttr = typeof(T).GetCustomAttribute<OptionsAttribute>();

            var collectionName = optionsAttr?.CollectionName ?? typeof(T).Name;
            var databaseName = optionsAttr?.DatabaseName ?? configuration.DatabaseName;

            var database = client.GetDatabase(databaseName);
            return database.GetCollection<ConfigDocument<T>>(collectionName);
        }
    }
}