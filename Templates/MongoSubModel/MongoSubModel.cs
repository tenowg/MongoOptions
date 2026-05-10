using MongoOptions.Attributes;
using System.Text.Json.Serialization;

[SubClass]
public partial class $safeitemname$
{
}

// Required for System.Text.Json source generator
[JsonSerializable(typeof($safeitemname$))]
public partial class $safeitemname$JsonContext : JsonSerializerContext { }

// Required for Microsoft Options validation source generator
// uncomment for Validator support, one DataAnnotation is required before you uncomment
//[OptionsValidator]
//public partial class $safeitemname$Validator : IValidateOptions<$safeitemname$> { }
