using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoOptions
{
    /// <summary>
    /// Provides extension methods for configuring MongoDB-based options in the dependency injection container.
    /// </summary>
    public static class MongoOptionsExtensions
    {
        /// <summary>
        /// Adds MongoDB-based configuration for the specified options type.
        /// This method registers the necessary services to load and manage options from MongoDB.
        /// </summary>
        /// <typeparam name="T">The type of options to configure, must be a class with a parameterless constructor.</typeparam>
        /// <param name="services">The service collection to add the configuration to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMongoOptions<T>(this IServiceCollection services) where T : class, new()
        {
            services.AddSingleton<IConfigureOptions<T>, MongoDbKeyedConfigurator<T>>();

            return services;
        }

        /// <summary>
        /// Adds MongoDB configuration with the specified options and returns a builder for further configuration.
        /// This sets up the MongoDB client, memory cache, and configuration manager.
        /// </summary>
        /// <param name="services">The service collection to add the configuration to.</param>
        /// <param name="configure">An action to configure the MongoDB options.</param>
        /// <returns>A MongoOptionsBuilder for registering additional options types.</returns>
        public static MongoOptionsBuilder AddMongoConfiguration(this IServiceCollection services, Action<MongoConfigurationOptions> configure)
        {
            var options = new MongoConfigurationOptions();
            configure(options);

            services.AddMemoryCache();
            services.AddSingleton<IMongoClient>(new MongoClient(options.ConnectionString));
            services.AddScoped<IConfigManager, MongoConfigManager>();

            services.AddSingleton(options);

            return new MongoOptionsBuilder(services);
        }
    }
}