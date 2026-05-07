namespace MongoOptions.Interfaces
{
    public interface IMongoLockScope : IAsyncDisposable
    {
        string HolderId { get; }
    }
}