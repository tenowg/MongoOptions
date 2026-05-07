namespace MongoOptions.Services
{
    [Serializable]
    internal class MongoLockAcquisitionException : Exception
    {
        public MongoLockAcquisitionException()
        {
        }

        public MongoLockAcquisitionException(string? message) : base(message)
        {
        }

        public MongoLockAcquisitionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}