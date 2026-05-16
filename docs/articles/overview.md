# Overview

**MongoOptions** is a high-performance configuration provider for .NET that uses MongoDB as a persistent backing store. It seamlessly integrates with the standard `IOptions` pattern while adding powerful features like real-time updates, distributed caching, metadata support, and optimistic concurrency control.

## What Problem Does It Solve?

Traditional .NET configuration is static—changes require application restarts or complex reload logic. MongoOptions solves this by storing configuration in MongoDB, allowing you to:

- Update configuration values at runtime without redeploying
- Manage tenant-specific or environment-specific settings dynamically
- Maintain version history and prevent race conditions with built-in locking
- Cache configuration locally for resilience during database outages

## Key Features

| Feature                    | Description                                                                 |
|---------------------------|-----------------------------------------------------------------------------|
| **Fluent API**            | Quick setup with a clean, readable configuration builder                    |
| **Real-time Updates**     | Automatic synchronization via `IOptionsMonitor<T>` and MongoDB change streams |
| **Named/Keyed Options**   | Support for multiple configuration instances (e.g., per-tenant settings)    |
| **Data Validation**       | Built-in Data Annotation validation to keep your configuration clean        |
| **CRUD Management**       | Full create/read/update/delete support via `IConfigManager`                 |
| **Metadata Search**       | Store and query configuration using custom metadata tags                    |
| **Distributed Locking**   | Prevent concurrent modifications with configurable write locks              |
| **Resilient Caching**     | "Stale-on-Failure" memory cache keeps your app running during DB downtime   |
| **Source Generation**     | Automatic registration of options using the `[MongoOption]` attribute       |

## How It Works

MongoOptions overrides the standard `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` interfaces. When you request configuration, it:

1. Checks the local memory cache first
2. Falls back to MongoDB if needed
3. Automatically refreshes cached values when changes are detected
4. Supports lazy loading for large collections via `IOptionsLazy<T>`

## When to Use MongoOptions

- You need dynamic configuration that can change without restarts
- You're building multi-tenant applications
- You want a single source of truth for configuration across multiple services
- You need audit trails or versioning for configuration changes
- You're already using MongoDB in your stack

## Next Steps

- [Getting Started](getting-started.md) – Install the package and configure your first options class
- Explore the [API Reference](/api/MongoOptions) – Detailed documentation for all public types and methods