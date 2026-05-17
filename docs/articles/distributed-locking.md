---
uid: distributed-locking
title: Distributed Locking
---

# Distributed Locking

MongoOptions provides a **distributed locking** mechanism that prevents multiple processes or users from modifying the same configuration simultaneously. It is especially useful when optimistic concurrency control alone is not sufficient.

## Why Use Distributed Locking?

In distributed environments, two services may attempt to update the same configuration at the same time. While optimistic concurrency control (OCC) detects conflicts after the fact, distributed locking provides a proactive way to acquire exclusive write access.

Use distributed locking when you need stronger guarantees, such as:

- Performing multi-step updates that must not be interrupted.
- Coordinating complex business logic that reads and then writes configuration.
- Avoiding frequent `MongoOptionsConcurrencyException` retries in high-contention scenarios.

## Acquiring a Lock

The recommended way to acquire a lock is with `LockScopedAsync<T>`. It returns an `IMongoLockScope` that automatically releases the lock when disposed.

```csharp
try
{
    await using var lockScope = await configManager.LockScopedAsync<FeatureSettings>(
        "TenantConfig_A", 
        duration: TimeSpan.FromMinutes(5));

    // Lock is held for the duration of this scope
    var settings = await configMonitor.Get<FeatureSettings>("TenantConfig_A");
    
    settings.Theme = "Dark";
    settings.MaxRetries = 10;

    await configManager.UpdateConfigAsync("TenantConfig_A", settings, lockScope: lockScope);
}
catch (MongoLockAcquisitionException ex)
{
    // Another process holds the lock
    Console.WriteLine($"Could not acquire lock: {ex.Message}");
}
```

### Manual Lock Management

For more control, you can use the lower-level methods:

```csharp
var result = await configManager.LockRecordAsync<FeatureSettings>(
    "TenantConfig_A", 
    duration: TimeSpan.FromMinutes(2));

if (result.Success)
{
    try
    {
        // Perform your work
        var settings = await configMonitor.Get<FeatureSettings>("TenantConfig_A");
        // ... modify settings ...

        await configManager.UpdateConfigAsync("TenantConfig_A", settings);
    }
    finally
    {
        await configManager.ReleaseLockAsync<FeatureSettings>("TenantConfig_A", result.HolderId);
    }
}
else
{
    Console.WriteLine(result.ErrorMessage);
}
```

You can also extend an existing lock:

```csharp
await configManager.RenewLockAsync<FeatureSettings>(
    "TenantConfig_A", 
    holderId, 
    extendBy: TimeSpan.FromMinutes(5));
```

## Lock Metadata

You can inspect the current lock state for debugging or monitoring:

```csharp
var lockInfo = await configManager.GetLock<FeatureSettings>("TenantConfig_A");

if (lockInfo?.LockedBy != null)
{
    Console.WriteLine($"Locked by {lockInfo.LockedBy} until {lockInfo.LockExpiresAt}");
}
```

## Exception: MongoLockAcquisitionException

```csharp
public class MongoLockAcquisitionException : Exception
```

Thrown by `LockScopedAsync<T>` when the lock cannot be acquired.

## Best Practices

- Prefer `using LockScopedAsync<T>` for automatic release and cleaner code.
- Keep lock durations short (typically 1–5 minutes) and use `RenewLockAsync` when longer operations are needed.
- Combine distributed locking with optimistic concurrency control for maximum safety.
- Always handle `MongoLockAcquisitionException` gracefully.
- Use the same `recordKey` (configuration name) consistently across all lock and update operations.
- Avoid holding locks across user interactions or long-running external calls.

## Related Topics

- [Optimistic Concurrency Control](optimistic-concurrency-control.md) – Version-based conflict detection
- [Getting Started](getting-started.md) – Registering options and using `IConfigManager`
- API Reference: @MongoOptions.Services.MongoConfigManager, `LockRecordAsync`, @MongoOptions.Interfaces.IMongoLockScope, @MongoOptions.Exceptions.MongoLockAcquisitionException