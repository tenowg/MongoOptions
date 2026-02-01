using System.Reflection;

namespace MongoOptions.Extensions
{
    public static class TypeExtensions
    {
        extension(Type type)
        {
            public bool IsNumber()
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                return type == typeof(int) || type == typeof(long) ||
                       type == typeof(double) || type == typeof(decimal);
            }

            public bool IsSimpleType()
            {
                return type.IsPrimitive ||
                       type == typeof(string) ||
                       type == typeof(decimal) ||
                       type == typeof(DateTime) ||
                       type == typeof(Guid);
            }

            public Type GetUnderlyingType()
                => Nullable.GetUnderlyingType(type) ?? type;

            public IEnumerable<PropertyInfo> GetEligibleProperties() =>
                type.GetProperties().Where(p => p.CanWrite && p.CanRead);

            public object? GetDefaultValue()
            {
                // 1. Strings are the special case
                if (type == typeof(string)) return string.Empty;

                // 2. Guids need a valid non-null value to be useful
                if (type == typeof(Guid)) return Guid.NewGuid();

                // 3. For everything else (int, bool, etc.), Activator or default works
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }

                return null;
            }
        }
    }
}