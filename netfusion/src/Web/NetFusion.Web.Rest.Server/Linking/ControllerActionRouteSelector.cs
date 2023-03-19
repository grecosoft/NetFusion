using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedTypeParameter
namespace NetFusion.Web.Rest.Server.Linking;

/// <summary>
/// Used to select a controller's action method for generating an URL model related link.
/// Unlike the ActionTemplateSelector, the metadata gathered by this selector contains the
/// model property values that should be evaluated at runtime to determine the corresponding
/// route values.  The resulting URL will be a complete URL with all route parameters replaced
/// with the corresponding model property values.
/// </summary>
public class ControllerActionRouteSelector<TController, TSource>
    where TController : ControllerBase
    where TSource : class
{
    private readonly LambdaExpression _action;

    public ControllerActionRouteSelector(LambdaExpression action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public ControllerActionLink CreateLink(string relationName)
    {
        var controllerAction = (MethodCallExpression)_action.Body;
        var link = new ControllerActionLink(relationName, controllerAction.Method);
            
        SetRouteValues(link, _action);
        return link;
    }
        
    // Associates the action method argument to the corresponding model property
    // used to populate the argument.  This is used when sending the route-info to
    // ASP.NET Core to resolve the corresponding URL.
    private static void SetRouteValues(ControllerActionLink resourceLink, LambdaExpression expression)
    {
        // The specified controller action declared method parameters:
        var controllerAction = (MethodCallExpression)expression.Body;
        var actionParams = controllerAction.Method.GetParameters()
            .OrderBy(p => p.Position)
            .ToArray();

        for (int i = 0; i < controllerAction.Arguments.Count; i++)
        {
            // The input argument to the action method parameter:
            var passedArg = controllerAction.Arguments[i];

            if (passedArg is MemberExpression propExpArg)
            {
                var propInfo = (PropertyInfo)propExpArg.Member;
                var actionParam = actionParams[i];
                var routeValue = new RouteParameter(actionParam.Name, propInfo);

                resourceLink.AddRouteValue(routeValue);
                continue;
            }

            // This is the case when the action parameter specifies a default value.
            if (passedArg is UnaryExpression unaryExpArg)
            {
                var operand = (MemberExpression)unaryExpArg.Operand;
                var propInfo = (PropertyInfo)operand.Member;
                var actionParam = actionParams[i];
                var routeValue = new RouteParameter(actionParam.Name, propInfo);

                resourceLink.AddRouteValue(routeValue);
            }
        }
    }  
}