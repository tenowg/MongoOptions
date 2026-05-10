using DnsClient.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoOptions.Data;
using MongoOptions.Exceptions;
using MongoOptions.Interfaces;
using MongoOptions.Types;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

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
        Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T value, Dictionary<string, object>? metadata = null) where T : class, IConfigFile;

        /// <summary>
        /// Updates a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration instance.</param>
        /// <param name="value">The configuration value to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string name, T value, Dictionary<string, object>? metadata = null, IMongoLockScope? lockScope = null) where T : class, IConfigFile;

        Task<LockAcquisitionResult> LockRecordAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, string? holderId = null, TimeSpan? duration = null) where T : class, IConfigFile;            // auto-generated if null

        Task<bool> RenewLockAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, string holderId, TimeSpan? extendBy = null) where T : class, IConfigFile;

        Task ReleaseLockAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, string holderId) where T : class, IConfigFile;

        Task<IMongoLockScope> LockScopedAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, TimeSpan? duration = null) where T : class, IConfigFile;

        /// <summary>
        /// This is for debug purposes only, and will be removed for preferred private access
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recordKey"></param>
        /// <returns></returns>
        Task<LockMetadata?> GetLock<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey) where T : class, IConfigFile;

        /// <summary>
        /// Retrieves all configuration keys for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="requiredMetadata">An optional metadata filter, checks for equality on all metadata entries</param>
        /// <returns>A list of configuration keys.</returns>
        Task<List<string>> GetKeys<T>(Dictionary<string, object>? requiredMetadata = null) where T : class, IConfigFile;

        /// <summary>
        /// Asynchronously retrieves the names of configuration documents that match the specified filter.
        /// </summary>
        /// <typeparam name="T">The type of configuration file. Must implement the IConfigFile interface.</typeparam>
        /// <param name="filterBuilder">A function that receives a FilterDefinitionBuilder for ConfigDocument<T> and returns a filter definition
        /// specifying which documents to match.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of strings with the names of
        /// the matching configuration documents. The list is empty if no documents match the filter.</returns>
        Task<List<string>> GetKeys<T>(
            Func<FilterDefinitionBuilder<ConfigDocument<T>>, FilterDefinition<ConfigDocument<T>>> filterBuilder)
            where T : class, IConfigFile;

        /// <summary>
        /// Determines whether a configuration file with the specified name and optional metadata exists in the
        /// underlying data store.
        /// </summary>
        /// <typeparam name="T">The type of configuration file to check for. Must implement the IConfigFile interface.</typeparam>
        /// <param name="configName">The name of the configuration file to search for. Cannot be null.</param>
        /// <param name="requiredMetadata">An optional dictionary of metadata key-value pairs that the configuration file must match. If null, only the
        /// name is used for the search.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a
        /// configuration file matching the specified criteria exists; otherwise, <see langword="false"/>.</returns>
        Task<bool> HasConfig<T>(string configName, Dictionary<string, object>? requiredMetadata = null) where T : class, IConfigFile;

        /// <summary>
        /// Determines whether a configuration document with the specified name exists and matches the given filter
        /// criteria.
        /// </summary>
        /// <remarks>This method queries the underlying MongoDB collection for a configuration document
        /// with the specified name and additional filter criteria. Returns false if the collection is not
        /// available.</remarks>
        /// <typeparam name="T">The type of the configuration file. Must implement the IConfigFile interface.</typeparam>
        /// <param name="configName">The name of the configuration document to search for. Cannot be null.</param>
        /// <param name="filterBuilder">A function that builds a filter definition to apply additional criteria to the search. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if a matching configuration
        /// document exists; otherwise, false.</returns>
        Task<bool> HasConfig<T>(string configName, Func<FilterDefinitionBuilder<ConfigDocument<T>>, FilterDefinition<ConfigDocument<T>>> filterBuilder) where T : class, IConfigFile;

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
        public async Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(T value, Dictionary<string, object>? metadata = null) where T : class, IConfigFile
        {
            await UpdateConfigAsync("Default", value, metadata);
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
        /// <exception cref="MongoOptionsConcurrencyException"></exception>
        public async Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string name, T value, Dictionary<string, object>? metadata = null, IMongoLockScope? lockScope = null) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            var validator = sp.GetService<IValidateOptions<T>>();
            var collection = connection.Collection;

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (validator != null)
            {
                var result = validator.Validate(nameof(T), value);

                if (result.Failed)
                {
                    throw new OptionsValidationException(name ?? MongoDefaultOptions.DefaultName, typeof(T), result.Failures);
                }
            }

            var locked = await GetLock<T>(name);

            if (locked != null && locked.LockedBy != lockScope?.HolderId)
            {
                throw new MongoOptionsConcurrencyException("We are locked this is for debug purposes we will handle this more gracefully later");
            }

            FilterDefinition<ConfigDocument<T>> filter;
            UpdateDefinition<ConfigDocument<T>> update;
            UpdateOptions options;
            int expectedVersion = 0;

            // we need a way to watch for non-watched versions this will break if not using Mongo Streams to update configs. Maybe.
            if (value.IsVersioned())
            {
                expectedVersion = value.GetVersion();
                value.SetVersion(expectedVersion + 1);

                filter = Builders<ConfigDocument<T>>.Filter.And(
                    Builders<ConfigDocument<T>>.Filter.Eq(d => d.Name, name),
                    Builders<ConfigDocument<T>>.Filter.Eq($"Value.{value.GetVersionPropertyName()}", expectedVersion));
                update = Builders<ConfigDocument<T>>.Update
                    .Set(d => d.Name, name)
                    .Set(d => d.Value, value);

                options = new UpdateOptions { IsUpsert = expectedVersion == 0 };
            } 
            else
            {
                filter = Builders<ConfigDocument<T>>.Filter.Eq(d => d.Name, name);
                update = Builders<ConfigDocument<T>>.Update
                    .Set(d => d.Name, name)
                    .Set(d => d.Value, value);
                
                options = new UpdateOptions { IsUpsert = true };
            }

            if (metadata != null)
            {
                update = update.Set(d => d.Metadata, metadata);
            }

            try
            {
                var resultDoc = await collection.UpdateOneAsync(filter, update, options);

                if (value.IsVersioned() && expectedVersion > 0 && resultDoc.MatchedCount == 0)
                {
                    throw new MongoOptionsConcurrencyException(
                        $"Configuration '{name}' was modified by another user. Expected version: {expectedVersion}.");
                }
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // This catches the exact scenario where TWO people tried to save a brand new 
                // config (expectedVersion == 0) at the exact same time, and one triggered the Upsert first.
                throw new MongoOptionsConcurrencyException(
                    $"Configuration '{name}' was just created by another user. Please reload.");
            }

        string cacheKey = $"{configuration.CachePrefix}{typeof(T).Name}_{name}";
            cache.Remove(cacheKey);
            IOptionsMonitorCache<T> optionsCache = sp.GetRequiredService<IOptionsMonitorCache<T>>();
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
        public async Task<List<string>> GetKeys<T>(Dictionary<string, object>? requiredMetadata = null) where T : class, IConfigFile
        {
            var connection = sp.GetService<IMongoConnection<T>>();
            if (connection == null) { return []; }

            var filter = Builders<ConfigDocument<T>>.Filter.Empty;

            filter = BuildMetaDataFilter(requiredMetadata, filter);

            var projection = Builders<ConfigDocument<T>>.Projection
                .Include(x => x.Name);

            return await connection.Collection
                .Find(filter)
                .Project(d => d.Name) //projection)
                .ToListAsync();

        }

        /// <summary>
        /// Asynchronously retrieves the names of configuration documents that match the specified filter.
        /// </summary>
        /// <typeparam name="T">The type of configuration file. Must implement the IConfigFile interface.</typeparam>
        /// <param name="filterBuilder">A function that receives a FilterDefinitionBuilder for ConfigDocument<T> and returns a filter definition
        /// specifying which documents to match.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of strings with the names of
        /// the matching configuration documents. The list is empty if no documents match the filter.</returns>
        public async Task<List<string>> GetKeys<T>(
            Func<FilterDefinitionBuilder<ConfigDocument<T>>, FilterDefinition<ConfigDocument<T>>> filterBuilder)
            where T : class, IConfigFile
        {
            var connection = sp.GetService<IMongoConnection<T>>();
            if (connection == null) { return []; }

            // Execute the caller's lambda to generate the filter
            var filter = filterBuilder(Builders<ConfigDocument<T>>.Filter);

            return await connection.Collection
                .Find(filter)
                .Project(d => d.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Removes a named configuration for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration options.</typeparam>
        /// <param name="name">The name of the configuration to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveConfig<T>(string name) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();

            await connection.Collection.DeleteOneAsync(o => o.Name == name);
        }

        /// <summary>
        /// Determines whether a configuration file with the specified name and optional metadata exists in the
        /// underlying data store.
        /// </summary>
        /// <typeparam name="T">The type of configuration file to check for. Must implement the IConfigFile interface.</typeparam>
        /// <param name="configName">The name of the configuration file to search for. Cannot be null.</param>
        /// <param name="requiredMetadata">An optional dictionary of metadata key-value pairs that the configuration file must match. If null, only the
        /// name is used for the search.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a
        /// configuration file matching the specified criteria exists; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> HasConfig<T>(string configName, Dictionary<string, object>? requiredMetadata = null) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();

            if (connection.Collection == null) return false;

            var filter = Builders<ConfigDocument<T>>.Filter.Eq(d => d.Name, configName);
            
            filter = BuildMetaDataFilter(requiredMetadata, filter);

            var result = await connection.Collection.FindAsync(filter);
            var item = result.FirstOrDefault();

            if (item == null || item.Name != configName) return false;
            return true;
        }

        /// <summary>
        /// Determines whether a configuration document with the specified name exists and matches the given filter
        /// criteria.
        /// </summary>
        /// <remarks>This method queries the underlying MongoDB collection for a configuration document
        /// with the specified name and additional filter criteria. Returns false if the collection is not
        /// available.</remarks>
        /// <typeparam name="T">The type of the configuration file. Must implement the IConfigFile interface.</typeparam>
        /// <param name="configName">The name of the configuration document to search for. Cannot be null.</param>
        /// <param name="filterBuilder">A function that builds a filter definition to apply additional criteria to the search. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if a matching configuration
        /// document exists; otherwise, false.</returns>
        public async Task<bool> HasConfig<T>(string configName, Func<FilterDefinitionBuilder<ConfigDocument<T>>, FilterDefinition<ConfigDocument<T>>> filterBuilder) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();

            if (connection.Collection == null) return false;

            var filter1 = Builders<ConfigDocument<T>>.Filter.Eq(d => d.Name, configName);

            var filter = filterBuilder(Builders<ConfigDocument<T>>.Filter);

            var result = await connection.Collection.FindAsync(Builders<ConfigDocument<T>>.Filter.And(filter1, filter));
            var item = result.FirstOrDefault();

            if (item == null || item.Name != configName) return false;
            return true;
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

            var source = await connection.Collection.Find(d => d.Name == sourceName).FirstOrDefaultAsync();

            if (source != null)
            {
                await UpdateConfigAsync(targetName, source.Value);
            }
        }
        
        private static FilterDefinition<T> BuildMetaDataFilter<T>(Dictionary<string, object>? requiredMetadata, FilterDefinition<T> filter) where T : class
        {
            if (requiredMetadata != null && requiredMetadata.Count > 0)
            {
                foreach (var kvp in requiredMetadata)
                {
                    filter &= Builders<T>.Filter.Eq(
                        $"Metadata.{kvp.Key}",
                        kvp.Value);
                }
            }

            return filter;
        }

        public async Task<LockAcquisitionResult> LockRecordAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, string? holderId = null, TimeSpan? duration = null) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            var validator = sp.GetService<IValidateOptions<T>>();
            var collection = connection.Collection ?? throw new Exception($"MongoCollection for {typeof(T).Name} is null");
            holderId ??= GenerateHolderId();

            var now = DateTime.UtcNow;
            var expiresAt = now.Add(duration ?? TimeSpan.FromMinutes(10));

            var filter = Builders<ConfigDocument<T>>.Filter.And(
                Builders<ConfigDocument<T>>.Filter.Eq(x => x.Name, recordKey),
                Builders<ConfigDocument<T>>.Filter.Or(
                    Builders<ConfigDocument<T>>.Filter.Eq(x => x.LockMetadata.LockedBy, null),
                    Builders<ConfigDocument<T>>.Filter.Lt(x => x.LockMetadata.LockExpiresAt, now)
                )
            );
            var update = Builders<ConfigDocument<T>>.Update
                .Set(x => x.LockMetadata.LockedBy, holderId)
                .Set(x => x.LockMetadata.LockExpiresAt, expiresAt)
                .Set(x => x.LockMetadata.LockAcquiredAt, now);

            var options = new FindOneAndUpdateOptions<ConfigDocument<T>>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = false
            };

            var result = await collection.FindOneAndUpdateAsync(filter, update, options);

            if (result != null)
            {
                return new LockAcquisitionResult
                {
                    Success = true,
                    HolderId = holderId,
                    ExpiresAt = expiresAt
                };
            }

            var current = await GetLock<T>(recordKey);
            var msg = current?.LockedBy != null
                ? $"Locked by '{current.LockedBy}' until {current.LockExpiresAt}"
                : "Failed to acquire lock (record may not exist)";

            return new LockAcquisitionResult
            {
                Success = false,
                ErrorMessage = msg,
                HolderId = current?.LockedBy ?? string.Empty,
                ExpiresAt = current?.LockExpiresAt
            };
        }

        public async Task<bool> RenewLockAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, string holderId, TimeSpan? extendBy = null) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            var validator = sp.GetService<IValidateOptions<T>>();
            var collection = connection.Collection ?? throw new Exception($"MongoCollection for {typeof(T).Name} is null");
            var now = DateTime.UtcNow;
            var expiresAt = now.Add(extendBy ?? TimeSpan.FromMinutes(10));

            var filter = Builders<ConfigDocument<T>>.Filter.And(
                Builders<ConfigDocument<T>>.Filter.Eq(x => x.Name, recordKey),
                Builders<ConfigDocument<T>>.Filter.Eq(x => x.LockMetadata.LockedBy, holderId),
                Builders<ConfigDocument<T>>.Filter.Gt(x => x.LockMetadata.LockExpiresAt, now)
            );
            var update = Builders<ConfigDocument<T>>.Update
                .Set(x => x.LockMetadata.LockedBy, holderId)
                .Set(x => x.LockMetadata.LockExpiresAt, expiresAt)
                .Set(x => x.LockMetadata.LockAcquiredAt, now);

            var options = new FindOneAndUpdateOptions<ConfigDocument<T>>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = false
            };

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task ReleaseLockAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey, string holderId) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            var collection = connection.Collection ?? throw new Exception($"MongoCollection for {typeof(T).Name} is null");

            var filter = Builders<ConfigDocument<T>>.Filter.And(
                Builders<ConfigDocument<T>>.Filter.Eq(x => x.Name, recordKey),
                Builders<ConfigDocument<T>>.Filter.Eq(x => x.LockMetadata.LockedBy, holderId)
            );

            var update = Builders<ConfigDocument<T>>.Update
                .Set(x => x.LockMetadata.LockedBy, null)
                .Set(x => x.LockMetadata.LockExpiresAt, null)
                .Set(x => x.LockMetadata.LockAcquiredAt, null);

            await collection.UpdateOneAsync(filter, update);
        }

        public async Task<LockMetadata?> GetLock<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string recordKey) where T : class, IConfigFile
        {
            var connection = sp.GetRequiredService<IMongoConnection<T>>();
            var collection = connection.Collection;

            var filter = Builders<ConfigDocument<T>>.Filter.Eq(x => x.Name, recordKey);

            var projection = Builders<ConfigDocument<T>>.Projection
                .Include(x => x.LockMetadata.LockedBy)
                .Include(x => x.LockMetadata.LockExpiresAt)
                .Include(x => x.LockMetadata.LockAcquiredAt);

            var result = await collection
                .Find(filter)
                .Project<ConfigDocument<T>>(projection)
                .FirstOrDefaultAsync();

            return result?.LockMetadata;
        }

        /// <summary>
        /// Acquires a lock and returns a scope that automatically releases it when disposed.
        /// Recommended for most use cases (cleanest syntax).
        /// </summary>
        public async Task<IMongoLockScope> LockScopedAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
            string recordKey,
            TimeSpan? duration = null)
            where T : class, IConfigFile
        {
            var result = await LockRecordAsync<T>(recordKey, holderId: null, duration);

            if (!result.Success)
            {
                // General-purpose exception (not chat-specific)
                throw new MongoLockAcquisitionException(
                    $"Failed to acquire lock for record '{recordKey}' of type {typeof(T).Name}. " +
                    $"Reason: {result.ErrorMessage}");
            }

            return new MongoRecordLockScope<T>(this, recordKey, result.HolderId);
        }

        private static string GenerateHolderId() =>
            $"{Environment.MachineName}-{Environment.ProcessId}-{Guid.NewGuid():N}";
    }
}