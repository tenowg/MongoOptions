
Please visit my [MongoOptions.Blazor](https://github.com/tenowg/MongoOptions.Blazor) Github, it will be a usuable project to add Razor components to manage your Config files

# MongoOptions.Core 🍃

A high-performance, resilient configuration provider for .NET 10 that uses MongoDB as a backing store with built-in memory caching and data validation.
Update your config files without reloading you project.

## 🚀 Features

* **Fluent Configuration**: Set up in seconds with a clean, readable API.
* **Resilient Caching**: Powered by `IMemoryCache` with "Stale-on-Failure" protection.
* **Keyed/Named Options**: Support for multiple configuration instances (e.g., Tenant-specific settings).
* **Data Validation**: Built-in support for Data Annotations to keep your DB clean.
* **Management API**: Full CRUD support for managing configs via code (perfect for Blazor Admin UIs).
* **Metadata Search**: Config files can be stored with metadata, and you can retrieve Keys based on metadata
* **Distributed Locking** Write Lock objects with custom Timeouts

## 📦 Installation

```bash
dotnet add package Tenowg.MongoOptions
```

## 🛠️ Quick Start

### 1. Define your Settings POCO

Use standard Data Annotations for validation and our custom attribute for DB naming.

```csharp
[MongoOption(DatabaseName = "AppSettings", CollectionName = "FeatureToggle")]
public partial class FeatureSettings
{
    [Required]
    public string Theme { get; set; } = "Light";

    [Range(1, 100)]
    public int MaxRetries { get; set; } = 5;
}

// this is currently optional, but if you want to use DataAnnotations for validation, it is required
// Source generation will fill in the rest of this class, you don't need to add anything else
[OptionsValidator]
public partial FeatureSettingsValidator : IValidateOption<FeatureSettings>
{}
```

MongoOption Attribute is required for Source Generation, you don't need to assign a custom Database name or CollectionName.

Source Generation has been added [MongoOptions.Generator](https://github.com/tenowg/MongoOptions.Generator). It is keyed to the attribute [MongoOptions]
So if you want to make your life even easier, just tag you POCOs with [MongoOption] and add .RegisterProjectNameDiscoveredOptions()
to your DI Configurtion and all your Configurations will magically be added.

```csharp
[MongoObject]
[MongoLazy]
public partial class FeatureList
{
    public List<string> List { get; set; } = [];
}
```
MongoLazy was introduced to work with List and other collections, it was designed to allow for adding to lists without loading the full data object into memory.

### 2. Register in Program.cs

Use the Fluent API to hook everything up.

```csharp
builder.Services.AddMongoConfiguration(config => 
{
    config.ConnectionString = "mongodb://localhost:27017";
    config.DatabaseName = "MyProductionApp";
})
~~.RegisterOptions<FeatureSettings>()~~
OR
.RegisterProjectNameDiscoveredOptions()
.RegisterOtherProjectDiscoveredOptions()
.AddMongoWatch();
```

When using the extension generated from codegen, it will make an extension for each project in your solution that references MongoOptions, this is the highly recommeneded way to add DI registration.

AddMongoWatch(): Uses Change Stream from MongoDB to update configs on Database change, your database needs to be a replica set.
This will be optional, as there is also an internal mechanism to handle changes. This is mostly useful for Multi server syncing.
This will be extendable, so look for Redis versions in the near future.

If you plan on using IOptionsMontor there is one more thing you need to add to your app.

BuildIndexes(): To improve the speed of Mongo Operations, you should use the BuildIndexes after you build. This will handle both Locks and options lookup.

```csharp
await app.BuildIndexes();
app.RunMongoMonitor();
```

It doesn't matter were you put it, but it needs to be there to clear the initial cache or monitor will not work.
You don't need this if you are only going to use IOptionsSnapshot.

### 3. Usage

Inject `IOptionsSnapshot<T>` for automatic updates every request, or `IOptionsMonitor<T>` for real-time changes.

```csharp
public class MyService(IOptionsSnapshot<FeatureSettings> settings)
{
    public void DoWork()
    {
        // Access the "Default" config
        var theme = settings.Value.Theme;

        // Access a "Named" config
        var tenantSettings = settings.Get("Tenant_A");
    }
}
```

Inject `IOptionsLazy<T>` to be able to append to Collections without loading the full data object, you don't need to 
inject both as in the example, useful if you have a Service that only adds to a list

```csharp
public FeaturesConstructor(IOptionLazy<FeatureList> list, IOptionsMonitor<FeatureList> object)
{
    public async Task AddToList(string configName, string entry)
    {
        await list.PushAsync(configName, x => x.List, entry);
    }

    public void UseTheDataLater(string configName)
    {
        var data = object.Get(configName);
        ...
    }
}
```

## 🛡️ Validation & Resilience

This library ensures your app never runs with "garbage" data.

1. **Strict Lookups**: If you request a named config that doesn't exist, it throws a `KeyNotFoundException`.
2. **Schema Protection**: If a MongoDB document fails Data Annotation validation, an `OptionsValidationException` is thrown.
3. **Database Downtime**: If MongoDB goes offline, the library will continue to serve cached data from the cache for 1 minute before trying the DB again, preventing "request spam."

## 🔧 Management (CRUD)

Use `IConfigManager` to build your own admin dashboard.

```csharp
// Get all available config names for a type
var keys = await _configManager.GetKeys<FeatureSettings>();

// Save or Update
await _configManager.UpdateConfigAsync("NewTenant", mySettingsObject);

// Remove
await _configManager.RemoveConfig<FeatureSettings>("OldTenant");

// Clone
await _configManager.CloneConfigAsync<FeatureSettings>("SourceTenant", "TargetTenant");
```

## Change Detection

Get immediate changes on change from the database, this only works with `IOptionsMonitor<T>`, it is very useful for syncing cross server, as all servers will receive the change notification as soon as
any change is made to the database entry.
```razor
@inject IOptionsMonitor<T> OptionsAccessor
@implements IDisposable

@code {
    private IDisposable? _onChangeToken;

    protected override async Task OnInitializedAsync()
    {
        _onChangeToken = OptionsAccessor.OnChange((e) => {
            LoadSelectedConfig();
            InvokeAsync(StateHasChanged);
       });
    }

    private void LoadSelectedConfig()
    {
        _editingModel = OptionsAccessor.Get(_selectedKey);
    }

    public void Dispose() => _onChangeToken?.Dispose();
}

```

## 📚 API Reference

### MongoOptionsExtensions
- `AddMongoConfiguration(Action<MongoConfigurationOptions>)`: Sets up MongoDB client and returns a builder.
- `AddMongoOptions<T>()`: Registers options for a type (alternative to builder).

### MongoOptionsBuilder
- `RegisterOptions<T>()`: Registers additional options types.

### IConfigManager
- `UpdateConfigAsync<T>(T)`: Updates default config.
- `UpdateConfigAsync<T>(string, T)`: Updates named config.
- `GetKeys<T>()`: Lists all config names for a type.
- `RemoveConfig<T>(string)`: Deletes a named config.
- `CloneConfigAsync<T>(string, string)`: Copies a config to a new name.

### MongoConfigurationOptions
- `ConnectionString`: MongoDB connection string.
- `DatabaseName`: Default database name.
- `CacheSoftDuration`: Cache refresh interval.
- `CacheHardDuration`: Max cache lifetime.
- `CachePrefix`: Cache key prefix.

### MongoOptionAttribute
- `CollectionName`: Custom collection name.
- `DatabaseName`: Custom database name.

## Building Locally

This project uses .NET 10.
You will need to pull [Tenowg.MongoOptions.Generator](https://github.com/tenowg/MongoOptions.Generator) and put it next to this project.
There is a good chance you will need to edit the .csproj file to fix some file paths.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

If you encounter any issues or have questions, please open an issue on GitHub.