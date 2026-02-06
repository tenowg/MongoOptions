using MongoDB.Driver;
using MongoOptions.Interfaces;

namespace MongoOptions.Services
{
    /// <summary>
    /// Registry for tracking registered configuration types.
    /// Provides methods to register and retrieve configuration types used in the MongoDB options system.
    /// </summary>
    public class MongoConfigRegistry(IEnumerable<IMongoConnection> connections)
    {
        // Key: The name/ID of the config, Value: The Type and a friendly name
        //private readonly HashSet<Type> _registeredConfigs = new();
        //private readonly ConcurrentDictionary<string, IConfigFile> _configs = new();
        private readonly Dictionary<string, IMongoConnection> _connections = connections.ToDictionary(o => o.Type.Name, o => o);

        /// <summary>
        /// Gets all registered configuration types.
        /// </summary>
        /// <returns>An enumerable collection of registered configuration types.</returns>
        public IEnumerable<Type> GetConfigs()
            => _connections.Select(o => o.Value.Type);

        public T GetInstance<T>() where T : IConfigFile
        {
            return (T)_connections.Where(o => o.Key == nameof(T)).Select(o => o.Value.Instance).FirstOrDefault()!;
        }

        public IConfigFile GetInstance(Type type)
        {
            return _connections.Where(o => o.Value.Type == type).Select(o => o.Value.Instance).FirstOrDefault()!;
        }

        public IConfigFile GetInstance(string type)
        {
            return _connections.Where(o => o.Key == type).Select(o => o.Value.Instance).FirstOrDefault()!;
        }
    }
}