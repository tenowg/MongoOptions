using MongoOptions.Types;

namespace MongoOptions.Interfaces
{
    public interface IDispatcher
    {
        object Execute<TProperty>(object model, PropertyMetadata prop);
    }

    public interface IDispatcherTwo
    {
        object Execute<TKey, TValue>(object model, PropertyMetadata prop);
    }

    public interface IClassDispatcher
    {
        object Execute<TProperty>(object model) where TProperty : class, IConfigFile, new();
    }

    public interface IClassDispatcherTwo
    {
        object Execute<TKey, TValue>(object model) where TValue : class, IConfigFile, new();
    }

    // This remains the 'gateway' for AOT to see the generic path
    public interface IDispatcherGateway<out TReturn>
    {
        TReturn Execute<T>(object model, PropertyMetadata prop);
        TReturn Execute<TKey, TValue>(object model, PropertyMetadata prop);
    }

    public interface IConfigDispatcherGateway<out TReturn>
    {
        TReturn Execute<T>(object model) where T : class, IConfigFile, new();
    }
}
