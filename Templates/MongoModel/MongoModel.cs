using Microsoft.Extensions.Options;
using MongoOptions.Attributes;
using System.Text.Json.Serialization;

// Your main model
[MongoOption]
[MongoLazy]
public partial class $safeitemname$
{
}

// Required for System.Text.Json source generator
[JsonSerializable(typeof($safeitemname$))]
public partial class $safeitemname$JsonContext : JsonSerializerContext { }

// Required for Microsoft Options validation source generator
//[OptionsValidator]
//public partial class $safeitemname$Validator : IValidateOptions <$safeitemname$> { }
