using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoOptions.Interfaces;
using MongoOptions.Types;
using System.Linq.Expressions;


namespace MongoOptions.Data
{
    public class MongoLazyConnection<T>(IMongoConnection<T> connection, IOptionsMonitorCache<T> optionsCache, IMemoryCache memCache, MongoConfigurationOptions configuration) : IOptionsLazy<T> where T : class, IConfigFile, new()
    {
        public IQueryable<T> AsQueryable(string name)
        {
            return connection.Collection.AsQueryable().Where(x => x.Name == name).Select(c => c.Value);
        }

        public async Task PushAsync<TItem>(
        string name,
        Expression<Func<T, IEnumerable<TItem>>> listSelector,  // supports List, ICollection, etc.
        TItem item,
        Expression<Func<ConfigDocument<T>, bool>>? securityFilter = null)
        {
            if (listSelector.Body is not MemberExpression memberExpr)
                throw new ArgumentException("Selector must be a simple property access (e.g., x => x.MyList).");

            string propertyName = memberExpr.Member.Name;

            // 2. Prepend "Value." to map it to your MongoDB wrapper document
            string mongoFieldPath = $"Value.{propertyName}";

            var filter = MongoLazyConnection<T>.BuildBaseFilter(name, securityFilter);

            // 3. MongoDB driver automatically casts the string to FieldDefinition!
            var update = Builders<ConfigDocument<T>>.Update
                .Push(mongoFieldPath, item);

            var result = await connection.Collection!.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new UnauthorizedAccessException($"Config '{name}' not found or access denied.");

            string cacheKey = $"{configuration.CachePrefix}{typeof(T).Name}_{name}";
            
            memCache.Remove(cacheKey);
            if (name == MongoDefaultOptions.DefaultName)
            {
                name = Options.DefaultName;
            }
            optionsCache.TryRemove(name);
        }

        private static FilterDefinition<ConfigDocument<T>> BuildBaseFilter(
        string name,
        Expression<Func<ConfigDocument<T>, bool>>? extraSecurity = null)
        {
            var filter = Builders<ConfigDocument<T>>.Filter.Eq(x => x.Name, name);

            if (extraSecurity != null)
                filter &= Builders<ConfigDocument<T>>.Filter.Where(extraSecurity);

            return filter;
        }
    }
}
