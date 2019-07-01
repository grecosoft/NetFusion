using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NetFusion.Rest.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable UnusedTypeParameter
namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Used to select a controller's action method for generating an URL resource related link.
    /// Unlike the ActionTemplateSelector, the metadata gathered by this selector contains the
    /// resource property values that should be evaluated at runtime to determine the corresponding
    /// route values.  The resulting URL will be a complete URL with all route parameters replaced
    /// with the corresponding resource property values.
    /// </summary>
    public class ActionUrlSelector<TController, TResource>
        where TController : ControllerBase
        where TResource : class, IResource
    {
        private readonly ActionUrlLink _link;
        private readonly LambdaExpression _action;

        public ActionUrlSelector(ActionUrlLink link, LambdaExpression action)
        {
            _action = action;
            _link = link;
        }

        public void SetRouteInfo()
        {
            SetControllerAndAction(_action);
            SetRouteValues(_action);
        }

        private void SetControllerAndAction(LambdaExpression action)
        {
            var controllerAction = (MethodCallExpression)action.Body;

            _link.Controller = controllerAction.Method.DeclaringType.Name;
            _link.Action = controllerAction.Method.Name;
            _link.Methods = GetHttpMethods(controllerAction.Method);
        }

        private static IEnumerable<string> GetHttpMethods(MemberInfo actionMethodInfo)
        {
            return actionMethodInfo.GetCustomAttributes<HttpMethodAttribute>(true)
                .SelectMany(a => a.HttpMethods);
        }

        // Associates the action method argument to the corresponding resource property
        // used to populate the argument.  This is used when sending the route-info to
        // ASP.NET Core to resolve the corresponding URL.
        private void SetRouteValues(LambdaExpression expression)
        {
            // The specified controller action declared method parameters:
            var controllerAction = (MethodCallExpression)expression.Body;
            var actionParams = controllerAction.Method.GetParameters()
                                               .OrderBy(p => p.Position)
                                               .ToArray();

            for (int i = 0; i < controllerAction.Arguments.Count(); i++)
            {
                // The input argument to the action method parameter:
                var passedArg = controllerAction.Arguments[i];

                if (passedArg is MemberExpression propExpArg)
                {
                    var propInfo = (PropertyInfo)propExpArg.Member;
                    var actionParam = actionParams[i];
                    var routeValue = new ActionParamValue(actionParam.Name, propInfo);

                    _link.AddRouteValue(routeValue);
                    continue;
                }

                // This is the case when the action parameter specifies a default value.
                if (passedArg is UnaryExpression unaryExpArg)
                {
                    var operand = (MemberExpression)unaryExpArg.Operand;
                    var propInfo = (PropertyInfo)operand.Member;
                    var actionParam = actionParams[i];
                    var routeValue = new ActionParamValue(actionParam.Name, propInfo);

                    _link.AddRouteValue(routeValue);
                }
            }
        }  
    }
}
