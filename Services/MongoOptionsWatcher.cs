using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Interfaces;
using System.Xml.Linq;

namespace MongoOptions.Services
{
    public class MongoOptionsWatcher(IServiceProvider sp, IEnumerable<IMongoConnection> connections) : BackgroundService
    {
        private record Database(string database, string collection);
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var databases = connections.DistinctBy(o => o.Database).Select(o => new Database(o.Database, o.CollectionName)).ToList();

            var tasks = databases.Select(db => RunTasks(db, cancellationToken));

            await Task.WhenAll(tasks);
        }

        private async Task RunTasks(Database database, CancellationToken cancellationToken)
        {
            //var configRegistry = sp.GetRequiredService<MongoConfigRegistry>();
            var mongoDatabase = sp.GetRequiredService<IMongoClient>().GetDatabase(database.database);
            var connections = sp.GetRequiredService<IEnumerable<IMongoConnection>>().Where(o => o.Database == database.database).ToDictionary(o => o.CollectionName, o => o);

            var options = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };

            using var watcher = await mongoDatabase.WatchAsync(options, cancellationToken: cancellationToken);

            while (await watcher.MoveNextAsync(cancellationToken))
            {
                foreach (var change in watcher.Current)
                {
                    
                    var collectionName = change.CollectionNamespace.CollectionName;
                    if (connections.TryGetValue(collectionName, out IMongoConnection? connection))
                    {
                        connection?.OnChanged(change.FullDocument);
                    }
                }
            }

        }
    }
}
