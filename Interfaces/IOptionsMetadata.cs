namespace MongoOptions.Interfaces
{
    public interface IOptionsMetadata<T>
    {
        Dictionary<string, object> GetMetadata(T metadata);
    }
}
