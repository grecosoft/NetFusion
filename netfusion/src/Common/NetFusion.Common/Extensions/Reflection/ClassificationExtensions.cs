using System;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection
{
    public static class ClassificationExtensions
    {
        // Types that are class based but consider to be primitive within this context.
        private static Type[] PrimitiveTypes { get; } = {typeof(string), typeof(DateTime)};
        
        public static bool IsBasicType(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            return propertyInfo.PropertyType.IsBasicType();
        }

        public static bool IsBasicType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsPrimitive || PrimitiveTypes.Contains(type);
        }
    }
}