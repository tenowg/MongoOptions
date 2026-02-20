using Microsoft.AspNetCore.Components;
using MongoOptions.Interfaces;

namespace MongoOptions.Types
{
    public record PropertyMetadata(
        string Name,
        string DisplayName,
        string Description,
        Type PropertyType,
        Func<object, object?> Getter,
        Action<object, object?> Setter,
        Func<object, object> ExpressionFactory,
        Func<object> New,
        Func<object> NewTypePropertyOne,
        Type GenericPropertyOneType,
        Func<object> NewTypePropertyTwo,
        Type GenericPropertyTwoType,
        Func<object, object, PropertyMetadata, object> AotDispatch
    )
    {
        public bool HasGenericPropertyOne => NewTypePropertyOne != null;
        public bool HasGenericPropertyTwo => NewTypePropertyTwo != null;
    };
}