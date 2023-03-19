using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions.Expressions;

namespace NetFusion.Web.Rest.Server.Linking;

/// <summary>
/// Selects a controller's action method used to find the corresponding route
/// template for a resource related link.  Controller action methods having up to
/// 5 parameters can be used.
/// </summary>
/// <typeparam name="TController">The controller to select action methods from
/// corresponding to template URLs.</typeparam>
[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public class ControllerActionTemplateSelector<TController>
    where TController : ControllerBase
{
    private readonly LambdaExpression _action;

    public ControllerActionTemplateSelector(LambdaExpression action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public TemplateUrlLink CreateLink(string relationName)
    {
        MethodInfo actionMethodInfo = _action.GetCallMethodInfo() ?? throw new InvalidOperationException(
            "Call Method Information could not be determined.");
            
        return new TemplateUrlLink(relationName, actionMethodInfo);
    }
}