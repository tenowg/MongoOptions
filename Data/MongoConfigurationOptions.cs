namespace MongoOptions.Data
{
    /// <summary>
    /// Configuration options for the MongoDB-based options provider.
    /// Defines connection settings, database names, and caching behavior.
    /// </summary>
    public class MongoConfigurationOptions
    {
        /// <summary>
        /// The MongoDB connection string used to connect to the database.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// The default database name to use for storing configuration documents.
        /// Defaults to "AppSettings".
        /// </summary>
        public string DatabaseName { get; set; } = "AppSettings";

        /// <summary>
        /// The duration for which configuration data is considered fresh in the cache.
        /// After this time, the cache will attempt to refresh from the database.
        /// Defaults to 10 minutes.
        /// </summary>
        public TimeSpan CacheSoftDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// The maximum duration for which configuration data is cached.
        /// After this time, the cache entry is evicted regardless of freshness.
        /// Defaults to 24 hours.
        /// </summary>
        public TimeSpan CacheHardDuration { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// The prefix used for cache keys to avoid collisions.
        /// Defaults to "mongo_cfg_".
        /// </summary>
        public string CachePrefix { get; set; } = "mongo_cfg_";
    }
}
