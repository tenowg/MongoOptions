namespace MongoOptions.Data
{
    public class LockMetadata
    {
        public string? LockedBy { get; set; }
        public DateTime? LockExpiresAt { get; set; }
        public DateTime? LockAcquiredAt { get; set; }
    }
}
