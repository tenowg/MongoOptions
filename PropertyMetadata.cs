using Microsoft.AspNetCore.Components;
using MongoOptions.Interfaces;

namespace MongoOptions
{
    public record PropertyMetadata(
        string Name,
        string DisplayName,
        string Description,
        Type PropertyType,
        Func<object, object?> Getter,
        Action<object, object?> Setter,
        Func<object, object> ExpressionFactory,
        Func<object, IDispatcher, PropertyMetadata, object> Dispatcher
        //Func<object, IDispatcher<object>, PropertyMetadata, object, object?> DispatcherWithState
    );
}