using Microsoft.AspNetCore.Mvc.Routing;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Extension methods for querying a controller's action methods.
    /// </summary>
    public static class ActionExtensions
    {
        /// <summary>
        /// Returns all actions methods for a controller.
        /// </summary>
        /// <param name="controller">The type of the controller.</param>
        /// <returns>Information for all action methods.</returns>
		public static IEnumerable<MethodInfo> GetActionMethods(this Type controller)
		{
            if (controller == null) throw new ArgumentNullException(nameof(controller),
                "Type cannot be null.");

			return controller.GetMethods(
				BindingFlags.DeclaredOnly |
				BindingFlags.Instance |
				BindingFlags.Public)
                .Where(m => m.HasAttribute<HttpMethodAttribute>());
		}

        /// <summary>
        /// Returns the controller's action method parameter type that is of a resource type.
        /// </summary>
        /// <param name="actionMethod">The controller's action method information.</param>
        /// <returns>The resource type being passed to the action method.</returns>
        public static Type GetActionRequestResourceType(MethodInfo actionMethod)
        {
            if (actionMethod == null) throw new ArgumentNullException(nameof(actionMethod));

            return actionMethod.GetParameters()
                .Select(p => p.ParameterType)
                .FirstOrDefault(pt => pt.IsDerivedFrom<IResource>());
        }

        /// <summary>
        /// Gets the resource type returned from a controller's method.
        /// </summary>
        /// <param name="methodInfo">The controller's action method information.</param>
        /// <returns>The resource type being returned from action method.  Returns null,
        /// if no resource is returned.</returns>
        public static Type GetActionResponseResourceType(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            if (methodInfo.ReturnType.IsDerivedFrom<IResource>())
            {
                return methodInfo.ReturnType;
            }

            if (methodInfo.ReturnType.IsClosedGenericTypeOf(typeof(Task<>), typeof(IResource)))
            {
                return methodInfo.ReturnType.GenericTypeArguments.First();
            }

            // This is the case if the action method returns a ASP.NET generic response type and
            // the action return type can't be determined by the method declaration.  The action
            // return type in this case can be specified via an attribute.
            var responseResourceAttrib = methodInfo.GetAttribute<ResourceTypeAttribute>();
            return responseResourceAttrib?.ResourceType;
        }
    }
}
