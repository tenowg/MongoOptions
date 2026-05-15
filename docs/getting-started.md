---
uid: getting-started
title: Getting Started
---

# Getting Started

This guide walks you through setting up MongoOptions in your .NET application.

## 1. Define Your Settings POCO

Use standard Data Annotations for validation and the `[MongoOption]` attribute to configure database naming.

```csharp
[MongoOption(DatabaseName = "AppSettings", CollectionName = "FeatureToggle", VersionPropertyName = "Version")]
public partial class FeatureSettings
{
    public int Version;

    [Required]
    public string Theme { get; set; } = "Light";

    [Range(1, 100)]
    public int MaxRetries { get; set; } = 5;
}

// Required when using DataAnnotations for validation
[OptionsValidator]
public partial class FeatureSettingsValidator : IValidateOption<FeatureSettings>
{
}
```

The `[MongoOption]` attribute is required for source generation. You do not need to assign custom `DatabaseName` or `CollectionName` values. The `VersionPropertyName` property enables optimistic concurrency control (OCC) protection and is optional.

Source generation is provided by the [MongoOptions.Generator](https://github.com/tenowg/MongoOptions.Generator) package. Tagging your POCOs with `[MongoOption]` and calling `.RegisterProjectNameDiscoveredOptions()` in your DI configuration will automatically register all discovered configurations.

For collection support, use the `[MongoLazy]` attribute:

```csharp
[MongoObject]
[MongoLazy]
public partial class FeatureList
{
    public List<string> List { get; set; } = [];
}
```

`[MongoLazy]` enables efficient append operations on collections without loading the full data object into memory.

## 2. Register in Program.cs

Use the fluent API to configure MongoOptions:

```csharp
builder.Services.AddMongoConfiguration(config =>
{
    config.ConnectionString = "mongodb://localhost:27017";
    config.DatabaseName = "MyProductionApp";
})
.RegisterProjectNameDiscoveredOptions()
.RegisterOtherProjectDiscoveredOptions()
.WithConfigIndices()
.AddMongoWatch();
```

- `RegisterProjectNameDiscoveredOptions()`: Recommended approach. The source generator creates an extension method for each project referencing MongoOptions.
- `WithConfigIndices()`: Creates database indexes on all configuration types to improve lookup performance. Highly recommended when using `IOptionsMonitor`.
- `AddMongoWatch()`: Enables real-time updates via MongoDB Change Streams. Requires a replica set. Useful for multi-server synchronization.

## 3. Usage

Inject `IOptionsSnapshot<T>` for per-request updates or `IOptionsMonitor<T>` for real-time change notifications.

```csharp
public class MyService(IOptionsSnapshot<FeatureSettings> settings)
{
    public void DoWork()
    {
        // Access the default configuration
        var theme = settings.Value.Theme;

        // Access a named configuration
        var tenantSettings = settings.Get("Tenant_A");
    }
}
```

For lazy collection operations, inject `IOptionsLazy<T>`:

```csharp
public class FeaturesConstructor(IOptionsLazy<FeatureList> list, IOptionsMonitor<FeatureList> monitor)
{
    public async Task AddToListAsync(string configName, string entry)
    {
        await list.PushAsync(configName, x => x.List, entry);
    }

    public void UseTheDataLater(string configName)
    {
        var data = monitor.Get(configName);
        // ...
    }
}