using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public static class ReflectionUtil
    {
        private static Type[] PrimitiveTypes { get; } = {typeof(string), typeof(DateTime)};
        
        public static bool IsPrimitiveProperty(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;

            return propType.IsPrimitive || PrimitiveTypes.Contains(propType);
        }

        public static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || PrimitiveTypes.Contains(type);
        }

        public static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;

            return propType.IsClass ||
                   propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        public static bool IsMarkedRequired(MemberInfo memberInfo) =>
            memberInfo.GetCustomAttribute<RequiredAttribute>() != null;


        public static bool IsEnumerableProperty(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;

            return propType.IsArray || propType.IsSubclassOf(typeof(IEnumerable));
        }

        public static Type GetEnumerableType(PropertyInfo propertyInfo)
        {
            Type propType = propertyInfo.PropertyType;
            if (propType.IsArray)
            {
                return propType.GetElementType();
            }

            if (propType.IsSubclassOf(typeof(IEnumerable<>)))
            {
                return propType.GetGenericArguments()[0];
            }

            return null;
        }
    }
}