using Microsoft.AspNetCore.Components;
using MongoOptions.Interfaces;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace MongoOptions.Types
{
    public record PropertyMetadata(
        string Name,
        string DisplayName,
        string Description,
        [DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] Type PropertyType,
        Func<object, object?> Getter,
        Action<object, object?> Setter,
        Func<object, object> ExpressionFactory,
        Func<object> New,
        Func<object> NewTypePropertyOne,
        Type GenericPropertyOneType,
        Func<object> NewTypePropertyTwo,
        Type GenericPropertyTwoType,
        Func<object, object, PropertyMetadata, object> AotDispatch,
        FrozenSet<string> Implements
    )
    {
        public bool HasGenericPropertyOne => NewTypePropertyOne != null;
        public bool HasGenericPropertyTwo => NewTypePropertyTwo != null;

        public bool CanAssignTo(Type openGeneric)
        {
            if (openGeneric is null) return false;
            if (!PropertyType.IsGenericType) return false;

            return Implements.Contains(openGeneric?.FullName ?? "");
        }
    };
}