using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using MongoDB.Driver;
using MongoOptions.Interfaces;

namespace MongoOptions.Services
{
    public class MongoOptionsWatcher(IServiceProvider sp, string database) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var configRegistry = sp.GetRequiredService<MongoConfigRegistry>();
            var mongoDatabase = sp.GetRequiredService<IMongoClient>().GetDatabase(database);
            var connections = sp.GetRequiredService<IEnumerable<IMongoConnection>>().Where(o => o.Database == database).ToDictionary(o => o.CollectionName, o => o);

            using var watcher = await mongoDatabase.WatchAsync(cancellationToken: cancellationToken);

            while (await watcher.MoveNextAsync(cancellationToken))
            {
                foreach (var change in watcher.Current)
                {
                    var collectionName = change.CollectionNamespace.CollectionName;
                    if (connections.TryGetValue(collectionName, out var connection))
                    {
                        // Trigger the update logic on the singleton
                        connection.OnChanged();
                    }
                }
            }

        }
    }
}
