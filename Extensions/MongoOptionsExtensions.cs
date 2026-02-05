using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Extensions;
using MongoOptions.Interfaces;

namespace MongoOptions.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring MongoDB-based options in the dependency injection container.
    /// </summary>
    public static class MongoOptionsExtensions
    {
        extension(IServiceCollection services)
        {
            /// <summary>
            /// Adds MongoDB-based configuration for the specified options type.
            /// This method registers the necessary services to load and manage options from MongoDB.
            /// </summary>
            /// <typeparam name="T">The type of options to configure, must be a class with a parameterless constructor.</typeparam>
            /// <param name="services">The service collection to add the configuration to.</param>
            /// <returns>The service collection for chaining.</returns>
            public IServiceCollection AddMongoOptions<T>() where T : class, IConfigFile, new()
            {
                new MongoOptionsBuilder(services).RegisterOptions<T>();

                return services;
            }

            /// <summary>
            /// Adds MongoDB configuration with the specified options and returns a builder for further configuration.
            /// This sets up the MongoDB client, memory cache, and configuration manager.
            /// </summary>
            /// <param name="services">The service collection to add the configuration to.</param>
            /// <param name="configure">An action to configure the MongoDB options.</param>
            /// <returns>A MongoOptionsBuilder for registering additional options types.</returns>
            public MongoOptionsBuilder AddMongoConfiguration(Action<MongoConfigurationOptions> configure)
            {
                var options = new MongoConfigurationOptions();
                configure(options);

                services.AddMemoryCache();
                services.AddSingleton<IMongoClient>(new MongoClient(options.ConnectionString));
                services.AddScoped<IConfigManager, MongoConfigManager>();
                services.AddSingleton<MongoConfigRegistry>();

                services.AddSingleton(options);

                return new MongoOptionsBuilder(services);
            }

            public T? FindOrAddRegisteredService<T>(ServiceLifetime lifetime = ServiceLifetime.Singleton) where T : class, new()
            {
                //var registryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T));

                T? service = services.FindRegisteredService<T>();

                if (service == null)
                {
                    service = new T();
                    
                    services.Add(new ServiceDescriptor(typeof(T), service));
                }

                return service;
            }

            public object? FindRegisteredService(Type type)
            {
                var registryDescriptor = services.FirstOrDefault(d => d.ServiceType == type);
                var existing = registryDescriptor?.ImplementationInstance;
                if (existing?.GetType() == type)
                {
                    return existing;
                }

                return null;
            }

            public T? FindRegisteredService<T>() where T : class, new()
            {
                return (T?)services.FindRegisteredService(typeof(T));
            }
        }

        public static IApplicationBuilder RunMongoMonitor(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var myService = scope.ServiceProvider.GetRequiredService<IEnumerable<IMongoConnection>>();

            foreach (var item in myService)
            {
                item.OnChanged();
            }

            return app;
        }
    }
}