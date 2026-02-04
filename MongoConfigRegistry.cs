using MongoDB.Driver;
using MongoOptions.Data;
using System.Collections.Concurrent;

namespace MongoOptions
{
    /// <summary>
    /// Registry for tracking registered configuration types.
    /// Provides methods to register and retrieve configuration types used in the MongoDB options system.
    /// </summary>
    public class MongoConfigRegistry
    {
        // Key: The name/ID of the config, Value: The Type and a friendly name
        private readonly HashSet<Type> _registeredConfigs = new();
        private readonly ConcurrentDictionary<string, IMongoDatabase> _databases = new();

        /// <summary>
        /// Registers a configuration type for use with MongoDB options.
        /// </summary>
        /// <typeparam name="TConfig">The type of the configuration options to register.</typeparam>
        public void Register<TConfig>() where TConfig : class
        {
            
            _registeredConfigs.Add(typeof(TConfig));
        }

        /// <summary>
        /// Gets all registered configuration types.
        /// </summary>
        /// <returns>An enumerable collection of registered configuration types.</returns>
        public IEnumerable<Type> GetConfigs()
            => _registeredConfigs;
    }
}