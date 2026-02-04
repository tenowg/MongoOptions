using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Extensions;
using MongoOptions.Interfaces;
using MongoOptions.Services;

namespace MongoOptions
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
        private MongoConfigurationOptions? options = services.FindOrAddRegisteredService<MongoConfigurationOptions>();

        /// <summary>
        /// Registers the specified options type for MongoDB-based configuration.
        /// Adds the configurator and options services to the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of options to register.</typeparam>
        /// <returns>The MongoOptionsBuilder for chaining.</returns>
        public MongoOptionsBuilder RegisterOptions<T>() where T : class, new()
        {
            services.AddSingleton<IConfigureOptions<T>, MongoDbKeyedConfigurator<T>>();
            services.AddSingleton<IOptionsChangeTokenSource<T>, MongoChangeTokenSource<T>>();
            services.AddSingleton<IMongoConnection<T>, MongoConnection<T>>();
            services.AddSingleton<IMongoConnection, MongoConnection<T>>();

            services.AddOptions<T>();

            var registry = services.FindOrAddRegisteredService<MongoConfigRegistry>();

            registry?.Register<T>();

            return this;
        }

        /// <summary>
        /// Adds Type to the Mongo ChangeStream, this is a default bahviour, additional Watch versions will be added
        /// as addtional packages. Requires mongo to be replica set, even single instance works
        /// </summary>
        /// <returns>The MongoOptionsBuilder for chaining.</returns>
        public MongoOptionsBuilder AddMongoWatch()
        {
            var registry = services.FindOrAddRegisteredService<MongoConfigRegistry>();
            var databaseName = registry?.GetConfigs().Select(o => o.GetDatabaseName(options!)).Distinct().ToList() ?? [];

            foreach(var group in databaseName)
            {
                services.AddKeyedSingleton(group, (sp, key) => new MongoOptionsWatcher(sp, group));
                services.AddHostedService(sp => sp.GetRequiredKeyedService<MongoOptionsWatcher>(group));
            }
            return this;
        }
    }
}
