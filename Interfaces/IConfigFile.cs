using MongoOptions.Types;

namespace MongoOptions.Interfaces
{
    public interface IConfigFile
    {
        IEnumerable<PropertyMetadata> GetProperties();
        Type GetConfigType();
        Type GetMonitorType();
        object Dispatcher(object model, IClassDispatcher receiver);
    }
}
