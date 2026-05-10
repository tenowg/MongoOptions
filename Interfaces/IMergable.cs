namespace MongoOptions.Interfaces
{
    public interface IMergable { }
    public interface IMergable<T> : IMergable where T : class
    {
        void Merge(T other);
        T Clone();
    }
}
