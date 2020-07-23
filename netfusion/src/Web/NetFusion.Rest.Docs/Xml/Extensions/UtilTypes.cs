using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Rest.Docs.Xml.Extensions
{
    /// <summary>
    /// Refection methods specific to documentation generation.
    /// </summary>
    public static class ReflectionUtil
    {
        // Types that are class based but consider to be primitive within this context.
        private static Type[] PrimitiveTypes { get; } = {typeof(string), typeof(DateTime)};
        
        public static bool IsPrimitiveProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            return IsPrimitiveType(propertyInfo.PropertyType);
        }

        public static bool IsPrimitiveType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive || PrimitiveTypes.Contains(type);
        }

        public static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            
            Type propType = propertyInfo.PropertyType;

            return propType.IsClass || propType.IsGenericType 
                && propType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        public static bool IsMarkedRequired(MemberInfo memberInfo)
        {
            if (memberInfo == null) throw new ArgumentNullException(nameof(memberInfo));
            return memberInfo.GetCustomAttribute<RequiredAttribute>() != null;
        }


        public static bool IsEnumerableProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            
            Type propType = propertyInfo.PropertyType;
            return propType.IsArray || 
                   propType.IsGenericType && 
                   propType.GetGenericArguments().Length == 1 &&
                   propType.CanAssignTo(typeof(IEnumerable));
        }

        public static Type GetEnumerableType(PropertyInfo propertyInfo)
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
}