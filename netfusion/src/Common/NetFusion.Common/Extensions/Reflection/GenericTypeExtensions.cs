using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable 1570
namespace NetFusion.Common.Extensions.Reflection;

/// <summary>
/// Extension methods used to query closed-generic types.
/// </summary>
public static class GenericTypeExtensions
{
    /// <summary>
    /// Determines if the specified type is an open generic type.
    /// </summary>
    /// <param name="sourceType">The type to check.</param>
    /// <returns>Returns true if the type is an open-generic type.</returns>
    /// <example>List<> would return true.  List<string> would return false.</example>
    public static bool IsOpenGenericType(this Type sourceType)
    {
        if (sourceType == null) throw new ArgumentNullException(nameof(sourceType));
            
        return sourceType.IsGenericType && sourceType.ContainsGenericParameters;
    }

    /// <summary>
    /// Determines if the specified type is a closed type of the specified open generic type.
    /// </summary>
    /// <param name="closedGenericType">The closed-generic type to check.</param>
    /// <param name="openGenericType">The open-generic type to test for.</param>
    /// <param name="specificClosedArgTypes">Optional.  If specified, the closed type
    /// arguments must be assignable to those listed.</param>
    /// <returns>True if the type if a closed generic type of the specified open generic type.</returns>
    public static bool IsClosedGenericTypeOf(this Type closedGenericType,
        Type openGenericType,
        params Type[] specificClosedArgTypes)
    {
        if (closedGenericType == null) throw new ArgumentNullException(nameof(closedGenericType));
        if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));
        
        if (! openGenericType.IsOpenGenericType())
        {
            throw new InvalidOperationException(
                $"The type of: {openGenericType} is not an open generic type.");
        }
        
        if (!closedGenericType.IsGenericType || closedGenericType.ContainsGenericParameters)
        {
            return false;
        }

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

        var openGenericTypeInfo = openGenericType.GetTypeInfo();

        if (openGenericTypeInfo.GenericTypeParameters.Length != specificClosedArgTypes.Length)
        {
            throw new InvalidOperationException(
                "The number of generic arguments of the open-generic type does not match the " +
                "number of specified closed-parameter types.");
        }

        var closedTypeArgTypes = closedGenericType.GetGenericArguments();
        for (int i = 0; i < specificClosedArgTypes.Length; i++)
        {
            var typeInfo = specificClosedArgTypes[i].GetTypeInfo();
            if (! typeInfo.IsAssignableFrom(closedTypeArgTypes[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Filters a list of types to only those implementing a closed interface of a specific open generic interface type.
    /// </summary>
    /// <param name="closedGenericTypes">The list of closed-generic types.</param>
    /// <param name="openGenericType">The open-generic interface type used to filter the list of closed generic types.</param>
    /// <param name="genericArgTypes">Optional.  If specified, the closed type arguments must be assignable to those listed.</param>
    /// <returns>Returns the type implementing the specified open-generic interface and the generics parameters of the matching
    /// closed interface.</returns>
    public static IEnumerable<GenericTypeInfo> WhereHavingClosedInterfaceTypeOf(this IEnumerable<Type> closedGenericTypes,
        Type openGenericType,
        params Type[] genericArgTypes)
    {
        if (closedGenericTypes == null) throw new ArgumentNullException(nameof(closedGenericTypes));
        if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));

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
                yield return new GenericTypeInfo(type, genericInterface.GetTypeInfo().GetGenericArguments());
            }
        }
    }
}

public class GenericTypeInfo
{
    public Type Type { get; }
    public Type[] GenericArguments { get; }

    public GenericTypeInfo(Type type, Type[] genericArguments)
    {
        Type = type;
        GenericArguments = genericArguments;
    }
}