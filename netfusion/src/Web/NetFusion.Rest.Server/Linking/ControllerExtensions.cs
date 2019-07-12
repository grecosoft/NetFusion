using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// Extension methods for querying a controller's action methods.
    /// </summary>
    public static class ControllerExtensions
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
    }
}
