using MongoOptions.Attributes;
using MongoOptions.Data;
using System.Reflection;

namespace MongoOptions.Extensions
{
    /// <summary>
    /// Provides extension methods for Type to support type analysis and default value generation.
    /// </summary>
    public static class TypeExtensions
    {
        extension(Type type)
        {
            /// <summary>
            /// Determines if the type is a supported numeric type for input operations.
            /// </summary>
            /// <returns>True if the type is a supported number type, otherwise false.</returns>
            public bool IsSupportedNumber()
            {
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                return underlyingType == typeof(int) ||
                       underlyingType == typeof(long) ||
                       underlyingType == typeof(double) ||
                       underlyingType == typeof(decimal) ||
                       underlyingType == typeof(float) ||
                       underlyingType == typeof(short);
                       //underlyingType == typeof(ushort);
                       //underlyingType == typeof(byte) ||
                       //underlyingType == typeof(sbyte);
                       //underlyingType == typeof(Half) ||
                       //underlyingType == typeof(Int128);
            }

            /// <summary>
            /// Determines if the type is a simple type that doesn't require complex serialization.
            /// </summary>
            /// <returns>True if the type is considered simple, otherwise false.</returns>
            public bool IsSimpleType()
            {
                return type.IsPrimitive ||
                       type == typeof(string) ||
                       type == typeof(decimal) ||
                       type == typeof(DateTime) ||
                       type == typeof(Guid);
            }

            /// <summary>
            /// Gets the underlying type if this is a nullable type, otherwise returns the type itself.
            /// </summary>
            /// <returns>The underlying type.</returns>
            public Type GetUnderlyingType()
                => Nullable.GetUnderlyingType(type) ?? type;

            /// <summary>
            /// Gets the properties of the type that are eligible for configuration (readable and writable).
            /// </summary>
            /// <returns>An enumerable collection of eligible properties.</returns>
            //public IEnumerable<PropertyInfo> GetEligibleProperties() =>
            //    type.GetProperties().Where(p => p.CanWrite && p.CanRead);

            /// <summary>
            /// Gets a default value for the type, suitable for initialization.
            /// </summary>
            /// <returns>A default value for the type, or null if no default is applicable.</returns>
            //public object? GetDefaultValue()
            //{
            //    if (type == typeof(string)) return string.Empty;

            //    if (type == typeof(Guid)) return Guid.NewGuid();

            //    if (type.IsValueType)
            //    {
            //        return Activator.CreateInstance(type);
            //    }

            //    return null;
            //}

            /// <summary>
            /// Gets a user-friendly type name for UI display purposes.
            /// </summary>
            /// <returns>A string representation of the type suitable for user interfaces.</returns>
            public string GetUITypeName()
            {
                // 1. Handle primitives and common types
                if (type == typeof(string)) return "string";
                if (type == typeof(int)) return "int";
                if (type == typeof(bool)) return "bool";
                if (type == typeof(double)) return "double";
                if (type == typeof(long)) return "long";
                if (type == typeof(decimal)) return "decimal";

                // 2. Handle Nullables (e.g., int? -> int?)
                var nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null)
                    return $"{GetUITypeName(nullableType)}?";

                // 3. Handle Generics (e.g., List<T> or Dictionary<K, V>)
                if (type.IsGenericType)
                {
                    var name = type.Name;
                    // Remove the `1 or `2 suffix
                    var index = name.IndexOf('`');
                    if (index > 0) name = name.Substring(0, index);

                    var genericArgs = type.GetGenericArguments()
                                          .Select(GetUITypeName);

                    return $"{name}<{string.Join(", ", genericArgs)}>";
                }

                // 4. Fallback to standard Name
                return type.Name;
            }

            public string GetDatabaseName(MongoConfigurationOptions options)
            {
                var optionsAttr = type.GetCustomAttribute<MongoOptionAttribute>();

                if (optionsAttr != null)
                {
                    return optionsAttr?.DatabaseName ?? options.DatabaseName;
                }

                return string.Empty;
            }
        }
    }
}