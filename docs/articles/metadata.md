---
uid: metadata
title: Metadata
---

# Metadata

Metadata is optional for all MongoObjects, it is easy to opt-in for metadata.

``` csharp
public record FeatureSettingsMetadata(string OwnerId, bool IsPublic) : IMetadataOptions;

[MongoOption]
[OptionsMetadata(typeof(FeatureSettingsMetadata))]
public partial class FeatureSettings
{
    [Required]
    public string Theme { get; set; } = "Light";

    [Range(1, 100)]
    public int MaxRetries { get; set; } = 5;
}
```

By adding `OptionsMetadata(typeof(YourRecord))` CodeGen is able to produce a typesafe, intellisense class you can use to query your objects based on metadata defined by the record.

## Usage

``` csharp
var recordIds = await manager.GetKeysAsyncFeatureSettings(
    f => f.IsPublic.Eq(false)
    .Or(
        f => f.OwnerId.Eq("someownerid"),
        f => f.OwnerId.Eq("anotherownerid")
    ));
```

Currently available is Lt (less than), Gt (greater than), Eq (equals). This will respect the types defined in the record, including Dates, bool, ints, string, etc.

## Related Topics

- [Getting Started](getting-started.md) – Registering options and using `IConfigManager`
- API Reference: @MongoOptions.Services.MongoConfigManager, @MongoOptions.Generator.MetadataFieldFilter`3