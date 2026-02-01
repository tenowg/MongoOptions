using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Xml.Linq;

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

            var registryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(MongoConfigRegistry));

            MongoConfigRegistry registry;

            if (registryDescriptor?.ImplementationInstance is MongoConfigRegistry existingRegistry)
            {
                registry = existingRegistry;
            }
            else
            {
                registry = new MongoConfigRegistry();
                services.AddSingleton(registry);
            }

            registry.Register<T>();

            return this;
        }
    }
}
