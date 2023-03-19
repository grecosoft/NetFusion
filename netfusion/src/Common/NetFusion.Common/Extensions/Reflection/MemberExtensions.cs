using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection;

public static class MemberExtensions
{
    public static bool IsNullable(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            
        Type propType = propertyInfo.PropertyType;

        return propType.IsClass || propType.IsGenericType 
            && propType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
        
    public static bool IsMarkedRequired(this MemberInfo memberInfo)
    {
        if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
        return memberInfo.GetCustomAttribute<RequiredAttribute>() != null;
    }
        
    public static bool IsEnumerable(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            
        Type propType = propertyInfo.PropertyType;
        return propType.IsArray || 
               propType.IsGenericType && 
               propType.GetGenericArguments().Length == 1 &&
               propType.CanAssignTo(typeof(IEnumerable));
    }

    public static Type? GetEnumerableType(this PropertyInfo propertyInfo)
    {
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            
        Type propType = propertyInfo.PropertyType;
        if (propType.IsArray)
        {
            return propType.GetElementType();
        }

        if (propType.IsGenericType && propType.GetGenericArguments().Length == 1 
                                   && propType.CanAssignTo(typeof(IEnumerable)))
        {
            return propType.GetGenericArguments()[0];
        }

        return null;
    }
}