using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions
{
    /// <summary>
    /// Extension methods for checking a type's structure.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Creates an instance of a type using the constructor with matching parameter types.
        /// </summary>
        /// <typeparam name="T">The type to instantiate.</typeparam>
        /// <param name="args">The arguments to pass to a matching constructor.</param>
        /// <returns>The created instance.</returns>
        public static T CreateInstance<T>(params object[] args)
        {
            return (T)Activator.CreateInstance(typeof (T), args);
        }

        /// <summary>
        /// Creates an instance of a type using the constructor with matching parameter types.
        /// </summary>
        /// <param name="type">The type object representing the type.</param>
        /// <param name="args">The arguments to pass to a matching constructor.</param>
        /// <returns>The created instance.</returns>
        public static object CreateInstance(this Type type, params object[] args)
        {
            Check.NotNull(type, nameof(type));

            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// Determines if a type derives from a base type.
        /// </summary>
        /// <typeparam name="T">The parent type.</typeparam>
        /// <param name="childType">The child type to check.</param>
        /// <returns>True if the type derives from the specified base type.
        /// otherwise false.</returns>
        public static bool IsDerivedFrom<T>(this Type childType)
        {
            Check.NotNull(childType, nameof(childType));

            return typeof(T).IsAssignableFrom(childType);
        }

        /// <summary>
        /// Determines if a type derives from a parent type.
        /// </summary>
        /// <param name="type">The child type to check.</param>
        /// <param name="baseType">The parent type.</param>
        /// <returns></returns>
        public static bool IsDerivedFrom(this Type childType, Type parentType)
        {
            Check.NotNull(childType, nameof(childType));
            Check.NotNull(parentType, nameof(parentType));

            return parentType.IsAssignableFrom(childType);
        }

        public static bool IsCreatableType(this Type type)
        {
            return type.IsClass && !type.IsGenericType 
                && !type.IsAbstract && type.HasDefaultConstructor();
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Determines if the specified type is an open generic type.
        /// </summary>
        /// <param name="sourceType">The type to check.</param>
        /// <returns>Returns true if the type is an open-generic type.</returns>
        /// <example>List<> would return true.  List<string> would return false.</string></example>
        public static bool IsOpenGenericType(this Type sourceType)
        {
            Check.NotNull(sourceType, nameof(sourceType));
            return sourceType.IsGenericType && sourceType.ContainsGenericParameters;
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

            if (!openGenericType.IsOpenGenericType())
            {
                throw new InvalidOperationException(
                    $"The type of: {openGenericType} is not an open generic type.");
            }

            if (!closedGenericType.IsGenericType) return false;

            // Test if the closed type is based on the same open type.
            var closedGenericTypeDef = closedGenericType.GetGenericTypeDefinition();
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

            var openGenericTypeInfo = IntrospectionExtensions.GetTypeInfo(openGenericType);
  
            if (openGenericTypeInfo.GenericTypeParameters.Length != specificClosedArgTypes.Length)
            {
                throw new InvalidOperationException(
                    "The number of generic arguments of the open-generic type does not match the " +
                    "number of specified closed-parameter types.");
            }

            var closedTypeArgTypes = closedGenericType.GetGenericArguments();
            for (int i = 0; i < specificClosedArgTypes.Length; i++)
            {
                if (!specificClosedArgTypes[i].IsAssignableFrom(closedTypeArgTypes[i]))
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

            if (!openGenericType.IsInterface || !openGenericType.IsGenericType)
            {
                throw new InvalidOperationException("");
            }

            foreach (Type type in closedGenericTypes)
            {
                var genericInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsClosedGenericTypeOf(openGenericType, genericArgTypes));

                if (genericInterface != null)
                {
                    yield return new GenericTypeInfo
                    {
                        Type = type,
                        GenericArguments = genericInterface.GetGenericArguments()
                    };
                }
            }
        }
    }
}
