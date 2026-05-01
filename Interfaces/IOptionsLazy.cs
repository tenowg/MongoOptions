using MongoOptions.Services;
using System.Linq.Expressions;

namespace MongoOptions.Interfaces
{
    public interface IOptionsLazy<T> where T : class, IConfigFile, new()
    {
        IQueryable<T> AsQueryable(string name);

        Task PushAsync<TItem>(
        string name,
        Expression<Func<T, IEnumerable<TItem>>> listSelector,  // supports List, ICollection, etc.
        TItem item,
        Expression<Func<ConfigDocument<T>, bool>>? securityFilter = null);
    }
}
