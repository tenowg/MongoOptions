using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MongoOptions
{
    /// <summary>
    /// A builder class for configuring multiple options types with MongoDB backing.
    /// Allows fluent registration of additional options after initial setup.
    /// </summary>
    public class MongoOptionsBuilder(IServiceCollection services)
    {
        /// <summary>
        /// Gets the underlying service collection for direct manipulation if needed.
        /// </summary>
        public IServiceCollection Services => services;

        /// <summary>
        /// Registers the specified options type for MongoDB-based configuration.
        /// Adds the configurator and options services to the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of options to register.</typeparam>
        /// <returns>The MongoOptionsBuilder for chaining.</returns>
        public MongoOptionsBuilder RegisterOptions<T>() where T : class, new()
        {
            services.AddSingleton<IConfigureOptions<T>, MongoDbKeyedConfigurator<T>>();
            services.AddOptions<T>();
            return this;
        }
    }
}
