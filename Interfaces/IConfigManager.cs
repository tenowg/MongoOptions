using MongoDB.Driver;
using MongoOptions.Data;
using System.Diagnostics.CodeAnalysis;

namespace MongoOptions.Interfaces
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

        Task UpdateConfigAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T, TMeta>(string name, T value, TMeta? metadata = null, IMongoLockScope? lockScope = null) where T : class, IConfigFile, IOptionsMetadata<TMeta> where TMeta : class, IMetadataOptions;

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
        /// <param name="filterBuilder">A function that receives a FilterDefinitionBuilder for ConfigDocument and returns a filter definition
        /// specifying which documents to match.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of strings with the names of
        /// the matching configuration documents. The list is empty if no documents match the filter.</returns>
        Task<List<string>> GetKeys<T>(
            Func<FilterDefinitionBuilder<ConfigDocument<T>>, FilterDefinition<ConfigDocument<T>>> filterBuilder)
            where T : class, IConfigFile;

        Task<List<string>> GetKeysBuilder<T, TMeta>(
            FilterDefinition<ConfigDocument<T>> filter)
            where T : class, IConfigFile, IOptionsMetadata<TMeta>;

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
}