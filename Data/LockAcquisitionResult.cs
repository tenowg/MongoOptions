using System;
using System.Collections.Generic;
using System.Text;

namespace MongoOptions.Data
{
    public record LockAcquisitionResult
    {
        public bool Success { get; init; }
        public string HolderId { get; init; } = string.Empty;
        public DateTime? ExpiresAt { get; init; }
        public string? ErrorMessage { get; init; }

        public static LockAcquisitionResult Failed(string message) => new() { Success = false, ErrorMessage = message };
    }
}
