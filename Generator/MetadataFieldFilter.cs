using MongoDB.Driver;
using MongoOptions.Data;
using MongoOptions.Interfaces;

namespace MongoOptions.Generator
{
    // The generator creates this to handle the operators cleanly
    public struct MetadataFieldFilter<TBuilder, TValue, T> where T : class, IConfigFile
    {
        private readonly TBuilder _parent;
        private readonly string _fieldName;
        private readonly List<FilterDefinition<ConfigDocument<T>>> _filters;

        public MetadataFieldFilter(TBuilder parent, List<FilterDefinition<ConfigDocument<T>>> filters, string fieldName)
        {
            _parent = parent;
            _filters = filters;
            _fieldName = fieldName;
        }

        public TBuilder Eq(TValue value)
        {
            _filters.Add(Builders<ConfigDocument<T>>.Filter.Eq(_fieldName, value));
            return _parent;
        }

        public TBuilder Gt(TValue value)
        {
            _filters.Add(Builders<ConfigDocument<T>>.Filter.Gt(_fieldName, value));
            return _parent;
        }

        public TBuilder Lt(TValue value)
        {
            _filters.Add(Builders<ConfigDocument<T>>.Filter.Lt(_fieldName, value));
            return _parent;
        }

        // You can also easily add .Gte(), .Lte(), .Ne(), .In() here!
    }
}
