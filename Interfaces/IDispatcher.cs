namespace MongoOptions.Interfaces
{
    public interface IDispatcher
    {
        object Execute<TProperty>(object model, PropertyMetadata prop);
    }

    public interface IClassDispatcher
    {
        object Execute<TProperty>(object model) where TProperty : class, IConfigFile, new();
    }
}
