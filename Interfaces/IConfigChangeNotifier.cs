namespace MongoOptions.Interfaces
{
    public interface IConfigChangeNotifier<T>
    {
        //Task NotifyAsync(string configName, string key);

        void OnChange(string name);
    }
}