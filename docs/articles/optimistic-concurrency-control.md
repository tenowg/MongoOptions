---
uid: optimistic-concurrency-control
title: Optimistic Concurrency Control
---

# Optimistic Concurrency Control

MongoOptions provides built-in **optimistic concurrency control (OCC)** to prevent race conditions when multiple processes or users attempt to update the same configuration simultaneously.

## Why Use Optimistic Concurrency Control?

In distributed systems, two services might read the same configuration, make independent changes, and write them back—overwriting each other's updates. OCC solves this by tracking a version number on each configuration document.

When an update is attempted:
- The current version in the database is compared against the version the client last read.
- If they match, the update succeeds and the version is incremented.
- If they differ, a `MongoOptionsConcurrencyException` is thrown, forcing the client to reload and retry.

## Enabling OCC

OCC is enabled per-options class by specifying the `VersionPropertyName` in the `[MongoOption]` attribute:

```csharp
[MongoOption(DatabaseName = "AppSettings", CollectionName = "FeatureToggle", VersionPropertyName = "Version")]
public partial class FeatureSettings
{
    public int Version; // Required for OCC – must be an int

    [Required]
    public string Theme { get; set; } = "Light";

    [Range(1, 100)]
    public int MaxRetries { get; set; } = 5;
}
```

- The version property must be a public `int` field or property.
- The source generator automatically implements `IConfigFile` methods (`GetVersion()`, `SetVersion(int)`, `IsVersioned()`, `GetVersionPropertyName()`) for you.

## How It Works

When you call `IConfigManager.UpdateConfigAsync<T>()`:

1. MongoOptions reads the current document and its version.
2. It performs an atomic update using a filter that matches both the name **and** the expected version.
3. On success, the version is incremented.
4. On conflict, `MongoOptionsConcurrencyException` is thrown.

Example update with conflict handling:

```csharp
try
{
    await configManager.UpdateConfigAsync("Default", mySettings);
}
catch (MongoOptionsConcurrencyException ex)
{
    // Reload the latest version and re-apply your changes
    var latest = await configMonitor.Get<FeatureSettings>("TenantConfig_A");
    // ... merge or prompt user ...
    await configManager.UpdateConfigAsync("TenantConfig_A", latest);
}
```

## Exception: MongoOptionsConcurrencyException

```csharp
public class MongoOptionsConcurrencyException : Exception
```

Thrown when:
- The configuration was modified by another process since it was last read.
- A new configuration was created concurrently during an upsert.

Message example:
> Configuration 'TenantConfig_A' was modified by another user. Expected version: 3.

## Best Practices

- Always handle `MongoOptionsConcurrencyException` in high-concurrency scenarios.
- Combine OCC with distributed locking (`LockScopedAsync`) when you need stronger guarantees.
- Keep the version field simple (`int Version`) – do not use it for application data.
- Use `IOptionsMonitor<T>` or `IOptionsSnapshot<T>` for read operations; they automatically receive the latest version.

## Related Topics

- [Getting Started](getting-started.md) – How to register options with `[MongoOption]`
- [Overview](overview.md) – Distributed locking and other features
- API Reference: @MongoOptions.Exceptions.MongoOptionsConcurrencyException, @MongoOptions.Services.MongoConfigManager