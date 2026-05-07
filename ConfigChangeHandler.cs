using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Interfaces;
using MongoOptions.Types;

namespace MongoOptions
{
    internal class ConfigChangeHandler<T>(IOptionsMonitorCache<T> optionsCache, IOptionsChangeTokenSource<T> optionsChange, IMongoCollection<ConfigDocument<T>> collection) : IConfigChangeNotifier<T> where T : class
    {
        private IChangeStreamCursor<ChangeStreamDocument<ConfigDocument<T>>> Watcher = collection.Watch<ConfigDocument<T>>();

        public void OnChange(string name)
        {
            string cacheKey = Options.DefaultName;

            if (name == MongoDefaultOptions.DefaultName)
            {
                cacheKey = Options.DefaultName;
            } else
            {
                cacheKey = name;
            }
                
            optionsCache.TryRemove(cacheKey);
        }
    }
}
