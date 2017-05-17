using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection
{
    public static class CreationReflectionExtensions
    {
        /// <summary>
        /// Determines if the type can have an instance created and has a default constructor.
        /// </summary>
        /// <param name="type">Type to be checked for default constructor instantiation.</param>
        /// <returns>True if an instance of the type can be created from default constructor.  Otherwise false.</returns>
        public static bool IsCreatableType(this Type type)
        {
            Check.NotNull(type, nameof(type));

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
            Check.NotNull(type, nameof(type));

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
            Check.NotNull(types, nameof(types));

            foreach (Type type in types.Where(t => t.IsCreatableType() && t.IsDerivedFrom<T>()).Distinct())
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
            Check.NotNull(types, nameof(types));
            Check.NotNull(baseType, nameof(baseType));

            foreach(Type type in types.Where(t => t.IsCreatableType() && t.IsDerivedFrom(baseType)).Distinct())
            {
                yield return type.CreateInstance();
            }
        }
    }
}
