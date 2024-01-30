using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection;

public static class ClassificationExtensions
{
    // Types that are class based but consider to be primitive within this context.
    private static IEnumerable<Type> PrimitiveTypes { get; } =
    [
        typeof(string), 
        typeof(DateTime),
        typeof(decimal)
    ];
        
    public static bool IsBasicType(this PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        return propertyInfo.PropertyType.IsBasicType();
    }

    public static bool IsBasicType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || PrimitiveTypes.Contains(type);
    }
}