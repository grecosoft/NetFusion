using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace NetFusion.Common.Extensions.Reflection
{
    public static class GenericTypeExtensions
    {
        /// <summary>
        /// Determines if the specified type is an open generic type.
        /// </summary>
        /// <param name="sourceType">The type to check.</param>
        /// <returns>Returns true if the type is an open-generic type.</returns>
        /// <example>List<> would return true.  List<string> would return false.</string></example>
        public static bool IsOpenGenericType(this Type sourceType)
        {
            Check.NotNull(sourceType, nameof(sourceType));

            var typeInfo = sourceType.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.ContainsGenericParameters;
        }

        /// <summary>
        /// Determines if the specified type is a closed type of the specified
        /// open generic type.
        /// </summary>
        /// <param name="closedGenericType">The closed-generic type to check.</param>
        /// <param name="openGenericType">The open-generic type to test.</param>
        /// <param name="specificClosedArgTypes">Optional.  If specified, the closed type
        /// arguments must be assignable to those listed.</param>
        /// <returns>True if the type if a closed generic type of the specified open
        /// generic type. </returns>
        public static bool IsClosedGenericTypeOf(this Type closedGenericType,
            Type openGenericType,
            params Type[] specificClosedArgTypes)
        {
            Check.NotNull(closedGenericType, nameof(closedGenericType));
            Check.NotNull(openGenericType, nameof(openGenericType));

            var closedGenericTypeInfo = closedGenericType.GetTypeInfo();

            if (!openGenericType.IsOpenGenericType())
            {
                throw new InvalidOperationException(
                    $"The type of: {openGenericType} is not an open generic type.");
            }

            if (!closedGenericTypeInfo.IsGenericType) return false;

            // Test if the closed type is based on the same open type.
            var closedGenericTypeDef = closedGenericTypeInfo.GetGenericTypeDefinition();
            if (closedGenericTypeDef != openGenericType)
            {
                return false;
            }

            // If no specific generic type parameters were specified, 
            // then the source type is a closed type of the generic type.
            if (specificClosedArgTypes.Length == 0)
            {
                return true;
            }

            var openGenericTypeInfo = openGenericType.GetTypeInfo();

            if (openGenericTypeInfo.GenericTypeParameters.Length != specificClosedArgTypes.Length)
            {
                throw new InvalidOperationException(
                    "The number of generic arguments of the open-generic type does not match the " +
                    "number of specified closed-parameter types.");
            }

            var closedTypeArgTypes = closedGenericTypeInfo.GetGenericArguments();
            for (int i = 0; i < specificClosedArgTypes.Length; i++)
            {
                var typeInfo = specificClosedArgTypes[i].GetTypeInfo();
                if (!typeInfo.IsAssignableFrom(closedTypeArgTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Filters a list of types to only those implementing an closed interface of a specific type.
        /// </summary>
        /// <param name="closedGenericTypes">The list of closed-generic types.</param>
        /// <param name="openGenericType">The open-generic interface type to determine if the closed-generic type is based.</param>
        /// <param name="genericArgTypes">Optional.  If specified, the closed type arguments must be assignable to those listed.</param>
        /// <returns></returns>
        static public IEnumerable<GenericTypeInfo> WhereHavingClosedInterfaceTypeOf(this IEnumerable<Type> closedGenericTypes,
            Type openGenericType,
            params Type[] genericArgTypes)
        {
            Check.NotNull(closedGenericTypes, nameof(closedGenericTypes));
            Check.NotNull(openGenericType, nameof(openGenericType));

            var openGenericTypeInfo = openGenericType.GetTypeInfo();

            if (!openGenericTypeInfo.IsInterface || !openGenericTypeInfo.IsGenericType)
            {
                throw new InvalidOperationException("Open generic interface type must be specified.");
            }

            foreach (Type type in closedGenericTypes)
            {
                var typeInfo = type.GetTypeInfo();

                var genericInterface = typeInfo.GetInterfaces()
                    .FirstOrDefault(i => i.IsClosedGenericTypeOf(openGenericType, genericArgTypes));

                if (genericInterface != null)
                {
                    yield return new GenericTypeInfo
                    {
                        Type = type,
                        GenericArguments = genericInterface.GetTypeInfo().GetGenericArguments()
                    };
                }
            }
        }
    }
}
