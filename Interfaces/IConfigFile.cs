using MongoOptions.Types;

namespace MongoOptions.Interfaces
{
    public interface IConfigFile
    {
        int __Mongo__Version { get; set; }
        IEnumerable<PropertyMetadata> GetProperties();
        Type GetConfigType();
        Type GetMonitorType();
        object Dispatcher(object model, object receiver);
    }
}
