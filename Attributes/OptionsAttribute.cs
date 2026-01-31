namespace MongoOptions.Attributes
{
    /// <summary>
    /// Attribute to specify custom database and collection names for configuration options.
    /// Applied to classes that represent configuration options to override default naming.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionsAttribute(string collectionName) : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the MongoDB collection to store the configuration.
        /// </summary>
        public string CollectionName { get; set; } = collectionName;

        /// <summary>
        /// Gets or sets the name of the MongoDB database to store the configuration.
        /// If null, the default database from MongoConfigurationOptions is used.
        /// </summary>
        public string? DatabaseName { get; set; }
    }
}