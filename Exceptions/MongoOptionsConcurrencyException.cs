namespace MongoOptions.Exceptions
{
    [Serializable]
    internal class MongoOptionsConcurrencyException : Exception
    {
        public MongoOptionsConcurrencyException()
        {
        }

        public MongoOptionsConcurrencyException(string? message) : base(message)
        {
        }

        public MongoOptionsConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}