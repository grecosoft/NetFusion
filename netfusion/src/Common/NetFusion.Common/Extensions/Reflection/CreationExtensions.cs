﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Common.Extensions.Reflection;

/// <summary>
/// Extension methods for creating object instances and for checking
/// related type properties.
/// </summary>
public static class CreationExtensions
{
    /// <summary>
    /// Determines if the type is a class that can be instantiated with a default constructor.
    /// </summary>
    /// <param name="type">Class type to be checked for default constructor instantiation.</param>
    /// <returns>True if the type is a class with a default constructor.  Otherwise False.</returns>
    public static bool IsCreatableClassType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type is { IsClass: true, IsGenericType: false, IsAbstract: false } && type.HasDefaultConstructor();
    }

    /// <summary>
    /// Creates an instance of a type using the constructor with matching parameter types.
    /// </summary>
    /// <param name="type">The type of the object to be created.</param>
    /// <param name="args">The arguments to pass to a matching constructor.</param>
    /// <returns>The created instance.</returns>
    public static object CreateInstance(this Type type, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(type);

        var instance = Activator.CreateInstance(type, args);
        if (instance == null)
        {
            throw new InvalidOperationException(
                $"The type {type} could not be created.");
        }

        return instance;
    }

    /// <summary>
    /// Filters the source list of types to those deriving from a specified base type 
    /// and creates an instance of each type.
    /// </summary>
    /// <typeparam name="T">The base type to filter the list of potential derived types.</typeparam>
    /// <param name="types">The list of potential derived types to filter.</param>
    /// <returns>Distinct list of object instances of all types deriving from the specified base type.</returns>
    public static IEnumerable<T> CreateInstancesDerivingFrom<T>(this IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        foreach (Type type in types.Where(t => t.IsCreatableClassType() && t.IsDerivedFrom<T>()).Distinct())
        {
            yield return (T)type.CreateInstance();
        }
    }

    /// <summary>
    /// Filters the source list of types to those deriving from a specified base type 
    /// and creates an instance of each type.
    /// </summary>
    /// <param name="types">The list of potential derived types to filter.</param>
    /// <param name="baseType">The base type to filter the list of potential derived types.</param>
    /// <returns>Distinct list of object instances of all types deriving from the specified base type.</returns>
    public static IEnumerable<object> CreateInstancesDerivingFrom(this IEnumerable<Type> types, Type baseType)
    {
        ArgumentNullException.ThrowIfNull(types);
        ArgumentNullException.ThrowIfNull(baseType);

        foreach (Type type in types.Where(t => t.IsCreatableClassType() && t.IsDerivedFrom(baseType)).Distinct())
        {
            yield return type.CreateInstance();
        }
    }
}