using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoOptions.Interfaces;

namespace MongoOptions.Services
{
    public class MongoOptionsStartupService(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider.GetRequiredService<IEnumerable<IMongoConnection>>();

            foreach (var item in services)
            {
                // Build indexes before the app starts taking traffic
                await item.EnsureIndices();

                // Start the change stream monitor
                item.OnStarted();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
