using MongoOptions.Types;

namespace MongoOptions.Interfaces
{
    public interface IConfigFile
    {
        bool IsVersioned();
        void SetVersion(int version);
        int GetVersion();
        string GetVersionPropertyName();
        IEnumerable<PropertyMetadata> GetProperties();
        Type GetConfigType();
        Type GetMonitorType();
        object Dispatcher(object model, object receiver);
    }
}
