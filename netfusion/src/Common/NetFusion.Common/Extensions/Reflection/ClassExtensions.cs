﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection
{
    /// <summary>
    /// Extension methods for querying type information.
    /// </summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// Determines if the type is a non-abstract type derived from the specified parent type.
        /// </summary>
        /// <param name="childType">The type to test if a derived concrete type.</param>
        /// <param name="parentType">The possible parent type of the child type.</param>
        /// <returns>True if the child type is not abstract and is derived from the parent type.</returns>
        public static bool IsConcreteTypeDerivedFrom(this Type childType, Type parentType)
        {
            if (childType == null) throw new ArgumentNullException(nameof(childType));
            if (parentType == null) throw new ArgumentNullException(nameof(parentType));

            return !childType.IsAbstract && childType.IsDerivedFrom(parentType);
        }

        /// <summary>
        /// Determines if the type is a non-abstract type derived from the specified parent type.
        /// </summary>
        /// <typeparam name="TParent">The possible parent type of the child type.</typeparam>
        /// <param name="childType">The type to test if a derived concrete type.</param>
        /// <returns>True if the child type is not abstract and is derived from the parent type.</returns>
        public static bool IsConcreteTypeDerivedFrom<TParent>(this Type childType)
        {
            if (childType == null) throw new ArgumentNullException(nameof(childType));
            return IsConcreteTypeDerivedFrom(childType, typeof(TParent));
        }

        /// <summary>
        /// Determines if a type derives from a parent type.
        /// </summary>
        /// <typeparam name="TParent">The parent type.</typeparam>
        /// <param name="childType">The child type to check.</param>
        /// <returns>True if the type derives from the specified base type otherwise false.</returns>
        public static bool IsDerivedFrom<TParent>(this Type childType)
        {
            if (childType == null) throw new ArgumentNullException(nameof(childType));
            return typeof(TParent).IsAssignableFrom(childType) && childType != typeof(TParent);
        }

        /// <summary>
        /// Determines if a type derives from a parent type.
        /// </summary>
        /// <param name="childType">The child type to check.</param>
        /// <param name="parentType">The parent type.</param>
        /// <returns>True if the type derives from the specified base type otherwise false.</returns>
        public static bool IsDerivedFrom(this Type childType, Type parentType)
        {
            if (childType == null) throw new ArgumentNullException(nameof(childType));
            if (parentType == null) throw new ArgumentNullException(nameof(parentType));

            return parentType.IsAssignableFrom(childType) && childType != parentType;
        }

        /// <summary>
        /// Determines if a child type can be assigned to a specified parent type.
        /// </summary>
        /// <param name="childType">The child type to test.</param>
        /// <param name="parentType">The parent type to test.</param>
        /// <returns>True if the child type is assignable to the parent type.  Otherwise, False.</returns>
        public static bool CanAssignTo(this Type childType, Type parentType)
        {
            if (childType == null) throw new ArgumentNullException(nameof(childType));
            if (parentType == null) throw new ArgumentNullException(nameof(parentType));

            return parentType.IsAssignableFrom(childType);
        }

        /// <summary>
        ///  Determines if a child type can be assigned to a specified parent type.
        /// </summary>
        /// <typeparam name="TParent">The parent type to test.</typeparam>
        /// <param name="childType">The child type to test.</param>
        /// <returns>True if the child type is assignable to the parent type.  Otherwise, False.</returns>
        public static bool CanAssignTo<TParent>(this Type childType)
        {
            if (childType == null) throw new ArgumentNullException(nameof(childType));
            return typeof(TParent).IsAssignableFrom(childType);
        }

        /// <summary>
        /// Returns all interfaces of a type deriving from a specified interface.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <param name="type">The type to check for interfaces.</param>
        /// <returns>Returns the list of matching interfaces.</returns>
        public static IEnumerable<Type> GetInterfacesDerivedFrom<T>(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (! typeof(T).IsInterface)
            {
                throw new InvalidOperationException(
                    "The specified generic parameter must be an interface type.");
            }

            return type.GetInterfaces()
                .Where(mi => mi.IsDerivedFrom<T>());
        }

        /// <summary>
        /// Determines if a specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type to verify.</param>
        /// <returns>True if the type has an empty constructor.  Otherwise, false.</returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Returns all method parameter types.
        /// </summary>
        /// <param name="methodInfo">Reference to a types method.</param>
        /// <returns>List of types.</returns>
        public static Type[] GetParameterTypes(this MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            return methodInfo.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
        }
    }
}
