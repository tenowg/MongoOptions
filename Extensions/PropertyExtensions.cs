using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MongoOptions.Extensions
{
    /// <summary>
    /// Provides extension methods for PropertyInfo to enhance property metadata handling.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        /// Gets the display name for a property, using the DisplayAttribute if available, otherwise the property name.
        /// </summary>
        /// <param name="prop">The PropertyInfo to get the display name for.</param>
        /// <returns>The display name of the property.</returns>
        extension(PropertyInfo prop)
        {
            public string GetDisplayName() =>
                prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;

            public string GetDescription() =>
                prop.GetCustomAttribute<DisplayAttribute>()?.Description ?? "";
        }
    }
}
