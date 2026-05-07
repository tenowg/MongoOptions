using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoOptions.Data
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

        public Dictionary<string, object> Metadata = [];

        public LockMetadata LockMetadata { get; set; } = new();

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
}