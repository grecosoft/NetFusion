using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection
{
    public static class CreationExtensions
    {
        /// <summary>
        /// Determines if the type is a class that can be instantiated with a default constructor.
        /// </summary>
        /// <param name="type">Class type to be checked for default constructor instantiation.</param>
        /// <returns>True if the type is a class with a default constructor.  Otherwise False.</returns>
        public static bool IsCreatableClassType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var typeInfo = type.GetTypeInfo();

            return typeInfo.IsClass && !typeInfo.IsGenericType
                && !typeInfo.IsAbstract && type.HasDefaultConstructor();
        }

        /// <summary>
        /// Creates an instance of a type using the constructor with matching parameter types.
        /// </summary>
        /// <param name="type">The type object representing the type.</param>
        /// <param name="args">The arguments to pass to a matching constructor.</param>
        /// <returns>The created instance.</returns>
        public static object CreateInstance(this Type type, params object[] args)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// Filters the source list of types to those that are assignable to the 
        /// provided base type and then creates an instance of each type.
        /// </summary>
        /// <typeparam name="T">The type or base type to filter the list of types.</typeparam>
        /// <param name="types">The list of types to filter.</param>
        /// <returns>Object instances of all plug-in types that are assignable to the specified types.</returns>
        public static IEnumerable<T> CreateInstancesDerivingFrom<T>(this IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));

            foreach (Type type in types.Where(t => t.IsCreatableClassType() && t.IsDerivedFrom<T>()).Distinct())
            {
                yield return (T)type.CreateInstance();
            }
        }

        /// <summary>
        /// Filters the source list of types to those that are assignable to the 
        /// provided base type and then creates an instance of each type.
        /// </summary>
        /// <param name="types">The list of types to filter.</param>
        /// <param name="baseType">The type or base type to filter the list of types.</param>
        /// <returns>Object instances of all plug-in types that are assignable to the specified types.</returns>
        public static IEnumerable<object> CreateInstancesDerivingFrom(this IEnumerable<Type> types, Type baseType)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (baseType == null) throw new ArgumentNullException(nameof(baseType));

            foreach (Type type in types.Where(t => t.IsCreatableClassType() && t.IsDerivedFrom(baseType)).Distinct())
            {
                yield return type.CreateInstance();
            }
        }
    }
}
