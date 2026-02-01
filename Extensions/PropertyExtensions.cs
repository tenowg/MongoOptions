using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MongoOptions.Extensions
{
    public static class PropertyExtensions
    {
        extension(PropertyInfo prop)
        {
            public Expression<Func<TProp>> CreateLambda<TProp>(object SettingsObject)
            {
                var constant = Expression.Constant(SettingsObject);
                var property = Expression.Property(constant, prop.Name);

                Expression finalExpression = property;
                if (prop.PropertyType != typeof(TProp))
                {
                    finalExpression = Expression.Call(property, typeof(object).GetMethod("ToString")!);
                }

                return Expression.Lambda<Func<TProp>>(finalExpression);
            }

            public Expression<Func<object>> CreateLambda(object SettingsObject, Type type)
            {
                var constant = Expression.Constant(SettingsObject);
                var property = Expression.Property(constant, prop.Name);

                Expression finalExpression = property;
                if (prop.PropertyType != type)
                {
                    finalExpression = Expression.Call(property, typeof(object).GetMethod("ToString")!);
                }

                return Expression.Lambda<Func<object>>(finalExpression);
            }

            public string GetDisplayName() =>
                prop.GetCustomAttribute<DisplayAttribute>()?.Name ?? prop.Name;
        }
    }
}
