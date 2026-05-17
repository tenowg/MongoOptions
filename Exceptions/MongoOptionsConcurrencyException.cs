namespace MongoOptions.Exceptions
{
    [Serializable]
    public class MongoOptionsConcurrencyException : Exception
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