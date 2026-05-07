using MongoOptions.Interfaces;
using MongoOptions.Services;
using System.Diagnostics.CodeAnalysis;

namespace MongoOptions.Data
{
    public class MongoRecordLockScope<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : IMongoLockScope where T : class, IConfigFile
    {
        private readonly IConfigManager _manager;
        private readonly string _recordKey;
        private readonly string _holderId;
        private bool _disposed;

        public string HolderId { get { return _holderId; } }

        internal MongoRecordLockScope(
            IConfigManager manager,
            string recordKey,
            string holderId)
        {
            _manager = manager;
            _recordKey = recordKey;
            _holderId = holderId;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await _manager.ReleaseLockAsync<T>(_recordKey, _holderId);
            }
            catch (Exception ex)
            {
                // Never throw from DisposeAsync - just log
                // You can inject ILogger if you want
                Console.WriteLine($"Warning: Failed to release lock for {_recordKey}: {ex.Message}");
                // Or use Debug.WriteLine / proper logger
            }
        }
    }
}
