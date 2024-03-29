﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Common.Extensions.Reflection;

/// <summary>
/// Extension methods for checking asynchronous method runtime information.
/// </summary>
public static class TaskMethodExtensions
{
    /// <summary>
    /// Determines if the method is an asynchronous operation.
    /// </summary>
    /// <param name="methodInfo">The method information to test.</param>
    /// <returns>True if the method returns a Task assignable type.</returns>
    public static bool IsTaskMethod(this MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        return methodInfo.ReturnType.CanAssignTo<Task>();
    }

    /// <summary>
    /// Determines if the method is an asynchronous operation returning a result.
    /// </summary>
    /// <param name="methodInfo">The method information to test.</param>
    /// <returns>True if the method returns a Task assignable type with generic result.</returns>
    public static bool IsTaskMethodWithResult(this MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        return IsTaskMethod(methodInfo) && methodInfo.ReturnType.GetTypeInfo().IsGenericType;
    }

    /// <summary>
    /// Determines if the method is an asynchronous operation that can be canceled.
    /// </summary>
    /// <param name="methodInfo">The method information to test.</param>
    /// <returns>True if the method returns a Task and takes a parameter assignable
    /// to CancellationToken.</returns>
    public static bool IsCancellableMethod(this MethodInfo methodInfo) 
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        bool isAsync = IsTaskMethod(methodInfo);
        if (! isAsync)
        {
            return false;
        }

        return methodInfo.GetParameterTypes()
            .Any(pt => pt.CanAssignTo<CancellationToken>());
    }
}