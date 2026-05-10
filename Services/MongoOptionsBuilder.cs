using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoOptions.Attributes;
using MongoOptions.Data;
using MongoOptions.Extensions;
using MongoOptions.Interfaces;
using System.Reflection;

namespace MongoOptions.Services
{
    /// <summary>
    /// A builder class for configuring multiple options types with MongoDB backing.
    /// Allows fluent registration of additional options after initial setup.
    /// </summary>
    public partial class MongoOptionsBuilder(IServiceCollection services)
    {
        /// <summary>
        /// Gets the underlying service collection for direct manipulation if needed.
        /// </summary>
        public IServiceCollection Services => services;
        private readonly MongoConfigurationOptions? options = services.FindOrAddRegisteredService<MongoConfigurationOptions>();

        /// <summary>
        /// Registers the specified options type for MongoDB-based configuration.
        /// Adds the configurator and options services to the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of options to register.</typeparam>
        /// <returns>The MongoOptionsBuilder for chaining.</returns>
        public MongoOptionsBuilder RegisterOptions<T>() where T : class, IConfigFile, new()
        {
            services.AddSingleton<IConfigureOptions<T>, MongoDbKeyedConfigurator<T>>();
            services.AddSingleton<IOptionsChangeTokenSource<T>, MongoChangeTokenSource<T>>();
            services.AddSingleton<IMongoConnection<T>, MongoConnection<T>>();
            services.AddSingleton<IMongoConnection, MongoConnection<T>>();

            var attr = typeof(T).GetCustomAttribute<MongoLazyAttribute>();
            if (attr != null)
                services.AddSingleton<IOptionsLazy<T>, MongoLazyConnection<T>>();

            services.AddOptions<T>();

            return this;
        }

        /// <summary>
        /// Add indexes to the database for performance, also handles clearing the cache so MongoWatch works correctly if using.
        /// </summary>
        /// <returns></returns>
        public MongoOptionsBuilder WithConfigIndices()
        {
            services.AddHostedService<MongoOptionsStartupService>();
            return this;
        }

        /// <summary>
        /// Adds Type to the Mongo ChangeStream, this is a default bahviour, additional Watch versions will be added
        /// as addtional packages. Requires mongo to be replica set, even single instance works
        /// </summary>
        /// <returns>The MongoOptionsBuilder for chaining.</returns>
        public MongoOptionsBuilder AddMongoWatch()
        {
            services.AddHostedService<MongoOptionsWatcher>();
            return this;
        }
    }
}
