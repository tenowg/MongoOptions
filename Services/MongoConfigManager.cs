using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoOptions.Data;
using MongoOptions.Interfaces;
using MongoOptions.Types;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MongoOptions.Services
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
        Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T value) where T : class, IConfigFile;

        /// <summary>
        /// Updates a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration instance.</param>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string name, T value) where T : class, IConfigFile;

        /// <summary>
        /// Retrieves all configuration keys for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <returns>A list of configuration keys.</returns>
        Task<List<string>> GetKeys<T>() where T : class, IConfigFile;

        //Task<List<string>> GetKeys(Type type);

        /// <summary>
        /// Removes a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveConfig<T>(string name) where T : class, IConfigFile;

        /// <summary>
        /// Clones a configuration from one name to another.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="sourceName">The name of the source configuration.</param>
        /// <param name="targetName">The name of the target configuration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CloneConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string sourceName, string targetName) where T : class, IConfigFile;
    }

    /// <summary>
    /// Implementation of IConfigManager for managing configuration options in MongoDB.
    /// Handles validation, caching, and database operations.
    /// </summary>
    public class MongoConfigManager(IServiceProvider sp, IMemoryCache cache, MongoConfigurationOptions configuration) : IConfigManager
    {
        /// <summary>
        /// Updates the default configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T value) where T : class, IConfigFile
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
        public async Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string name, T value) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            var validator = sp.GetService<IValidateOptions<T>>();
            var collection = connection.Collection;

            if (validator != null)
            {
                var result = validator.Validate(nameof(T), value);

                if (result != null)
                {
                    throw new OptionsValidationException(name ?? MongoDefaultOptions.DefaultName, typeof(T), result.Failures);
                }
            }

            var document = new ConfigDocument<T> { Name = name, Value = value };
            await collection.ReplaceOneAsync(
                filter: d => d.Name == name,
                replacement: document,
                options: new ReplaceOptions { IsUpsert = true }
            );

            string cacheKey = $"{configuration.CachePrefix}{typeof(T).Name}_{name}";
            cache.Remove(cacheKey);
            IOptionsMonitorCache<T> optionsCache = sp.GetRequiredService<IOptionsMonitorCache<T>>();
            MongoChangeTokenSource<T>? tokenSource = sp.GetRequiredService<IOptionsChangeTokenSource<T>>() as MongoChangeTokenSource<T>;
            if (name  == MongoDefaultOptions.DefaultName)
            {
                name = Options.DefaultName;
            }
            optionsCache.TryRemove(name);
        }

        /// <summary>
        /// Retrieves all configuration keys for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <returns>A list of configuration keys.</returns>
        public async Task<List<string>> GetKeys<T>() where T : class, IConfigFile
        {
            var connection = sp.GetService<IMongoConnection<T>>();
            if (connection == null) { return []; }

            return await connection.Collection.AsQueryable().Select(o => o.Name).ToListAsync() ?? [];
        }

        //public async Task<List<string>> GetKeys(Type type)
        //{
        //    //var connection = sp.GetService<IMongoConnection<T>>();
        //    //if (connection == null) { return []; }

        //    //return await connection.Collection.AsQueryable().Select(o => o.Name).ToListAsync() ?? [];
        //    return [];
        //}

        /// <summary>
        /// Removes a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveConfig<T>(string name) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            //var collection = GetCollection<T>();

            await connection.Collection.DeleteOneAsync(o => o.Name == name);
        }

        /// <summary>
        /// Clones a configuration from one name to another.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="sourceName">The name of the source configuration.</param>
        /// <param name="targetName">The name of the target configuration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CloneConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string sourceName, string targetName) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            //var collection = GetCollection<T>();

            var source = await connection.Collection.Find(d => d.Name == sourceName).FirstOrDefaultAsync();

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
        //private IMongoCollection<ConfigDocument<T>> GetCollection<T>()
        //{
        //    var optionsAttr = typeof(T).GetCustomAttribute<MongoOptionAttribute>();

        //    var collectionName = optionsAttr?.CollectionName ?? typeof(T).Name;
        //    var databaseName = optionsAttr?.DatabaseName ?? configuration.DatabaseName;

        //    var database = client.GetDatabase(databaseName);
        //    return database.GetCollection<ConfigDocument<T>>(collectionName);
        //}
    }
}