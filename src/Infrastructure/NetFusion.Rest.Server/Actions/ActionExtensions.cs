using Microsoft.AspNetCore.Mvc.Routing;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Extension methods for querying a controller's action methods.  Also used
    /// to find action methods matching a specified set of conventions.
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
            if (controller == null)
                throw new ArgumentNullException(nameof(controller), "Type not specified.");

			return controller.GetMethods(
				BindingFlags.DeclaredOnly |
				BindingFlags.Instance |
				BindingFlags.Public)
                .Where(m => m.HasAttribute<HttpMethodAttribute>());
		}

        /// <summary>
        /// Allows a controller's methods to be queried.  This is used when finding controller action
        /// based on a given convention (i.e. typical CRUD API methods for a resource type).
        /// </summary>
        /// <param name="controllerMethods">The list of controller methods.</param>
        /// <param name="resourceType">The resource type for which action methods are being queried.</param>
        /// <param name="httpMethods">HTTP methods the action(must support at least one).</param>
        /// <param name="acceptsResourceArg">Action method must take an argument of the resource type.</param>
        /// <param name="acceptsIdentityArg">Action method must take an argument identified as the resource identity.</param>
        /// <param name="returnsResourceType">Action method must return the resource type.</param>
        /// <returns>Information on all matching controller methods.</returns>
        public static ActionMethodInfo[] FindActionMethods(this IEnumerable<MethodInfo> controllerMethods,
            Type resourceType,
            HttpMethod[] httpMethods,
            bool? acceptsResourceArg = null,
            bool? acceptsIdentityArg = null,
            bool? returnsResourceType = null)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType), "Resource type not specified.");

            if (httpMethods == null)
                throw new ArgumentNullException(nameof(httpMethods), "HTTP methods not specified.");

            var results = controllerMethods.Select(cm => GetActionMethodCallInfo(cm, resourceType)).ToList();

            var filteredResults = results.Where(pi =>
                       (acceptsResourceArg == null || pi.AcceptsResourceArg == acceptsResourceArg)
                    && (acceptsIdentityArg == null || pi.AcceptsIdentityParam == acceptsIdentityArg)
                    && (returnsResourceType == null || pi.ReturnsResourceType == returnsResourceType)
                    && httpMethods.Any(m => pi.HttpMethods.Contains(m.Method)))
                .SelectActionInfo()
                .ToArray();

            return filteredResults;
        }

        /// <summary>
        /// Returns the controller's action method parameter type that is of a resource type.
        /// </summary>
        /// <param name="actionMethod">The controller's action method information.</param>
        /// <returns>The resource type being passed to the action method.</returns>
        public static Type GetActionRequestResourceType(MethodInfo actionMethod)
        {
            return actionMethod.GetParameters()
                .Select(p => p.ParameterType)
                .FirstOrDefault(pt => pt.IsDerivedFrom<IResource>());
        }

        /// <summary>
        /// Returns the resource type returned from a controller's method.
        /// </summary>
        /// <param name="methodInfo">The controller's action method information.</param>
        /// <returns>The resource type being returned from action method.  Returns null,
        /// if no resource is returned.</returns>
        public static Type GetActionResponseResourceType(MethodInfo methodInfo)
        {
            if (methodInfo.ReturnType == null)
            {
                return null;
            }

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

        /// <summary>
        /// Creates a list of ActionUrlLinks from a list of ActionMethodInfo instances describing a set of controller 
        /// action methods found based on conventions.  These links can be returned to the client and used to invoke 
        /// the corresponding action method.
        /// </summary>
        /// <param name="source">The action method information used to generate action links.</param>
        /// <returns>Link metadata used to generate resource specific links.</returns>
        public static IEnumerable<ActionUrlLink> ToActionLink(this IEnumerable<ActionMethodInfo> source)
        {
            var validConventionBasedHttpMethods = new[] { HttpMethod.Post.Method, HttpMethod.Delete.Method };

            foreach (ActionMethodInfo methodInfo in source)
            {
                ActionUrlLink actionLink = null;
                if (methodInfo.IdentityParamValue != null)
                {
                    actionLink = new ActionUrlLink(methodInfo.IdentityParamValue);
                }
                else if (methodInfo.Methods.Any(m => validConventionBasedHttpMethods.Contains(m)))
                {
                    actionLink = new ActionUrlLink();
                }

                if (actionLink != null)
                {
                    actionLink.Action = methodInfo.ActionMethodName;
                    actionLink.Controller = methodInfo.ControllerName;
                    actionLink.Methods = methodInfo.Methods;

                    yield return actionLink;
                }
            }
        }

        // Returns an instance of an object containing the details for a given action method that
        // can be searched to find action methods based on know conventions.
        private static ActionMethodSearchInfo GetActionMethodCallInfo(MethodInfo actionMethod, Type resourceType)
        {
            var callInfo = new ActionMethodSearchInfo
            {
                ResourceType = resourceType,
                AcceptsResourceArg = IsActionMethodCalledWithResource(actionMethod, resourceType),
                ActionMethod = actionMethod,
                ActionResourceIdentityValue = GetActionResourceIdentityValue(actionMethod, resourceType),
                HttpMethods = GetHttpMethods(actionMethod),
                ReturnsResourceType = ReturnsResource(actionMethod, resourceType)
            };

            callInfo.AcceptsIdentityParam = callInfo.ActionResourceIdentityValue != null;
            return callInfo;
        }

        private static bool IsActionMethodCalledWithResource(MethodInfo actionMethod, Type resourceType)
        {
            var resourceParams = actionMethod.GetParameters().Where(p => p.ParameterType == resourceType);
            return resourceParams.Count() == 1;
        }

        // Returns an object instance containing the property on the resource and the associated action method
        // argument corresponding to the identity value of the resource.
        private static ActionParamValue GetActionResourceIdentityValue(MethodInfo actionMethod, Type resourceType)
        {
            var resourceProps = resourceType.GetProperties(
                BindingFlags.GetProperty |
                BindingFlags.Instance | 
                BindingFlags.Public);

            var resourceIdentityProps = resourceProps
                .Where(p => IsResourceIdentityName(p.Name, resourceType));

            var actionIdentityArgs = actionMethod.GetParameters()
                .Where(p => IsResourceIdentityName(p.Name, resourceType));

            // There must be exactly one match for the resource property and action
            // method to be considered a identity value.
            PropertyInfo resourceIdentityProp = resourceIdentityProps.OneAndOnlyOne();
            ParameterInfo actionIdentityParam = actionIdentityArgs.OneAndOnlyOne();

            if (resourceIdentityProp?.PropertyType == actionIdentityParam?.ParameterType)
            {               
                return new ActionParamValue(actionIdentityParam.Name, resourceIdentityProp);
            }

            return null;
        }

        // Conventions used to identify resource property and corresponding action method identity parameter.
        private static bool IsResourceIdentityName(this string name, Type resourceType)
        {
            return name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                || name.Equals($"{resourceType.Name}Id", StringComparison.OrdinalIgnoreCase)
                || name.Equals($"{resourceType.Name.Replace("Resource", "")}Id", StringComparison.OrdinalIgnoreCase)
                || name.Equals($"{resourceType.Name.Replace("Model", "")}Id", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] GetHttpMethods(MethodInfo actionMethodInfo)
        {
            return actionMethodInfo.GetCustomAttributes<HttpMethodAttribute>(false)
                .SelectMany(a => a.HttpMethods)
                .ToArray();
        }

        private static bool ReturnsResource(this MethodInfo source, Type resourceType)
        {
            Type returnResourceType = GetActionResponseResourceType(source);
            return returnResourceType == resourceType;           
        }

        // Selects the search results into a class returned to the consumer executing the search.
        private static IEnumerable<ActionMethodInfo> SelectActionInfo(
            this IEnumerable<ActionMethodSearchInfo> actionMethods)
        {
            return actionMethods.Select(m => new ActionMethodInfo
            {
                ControllerName = m.ActionMethod.DeclaringType.Name,
                ActionMethodName = m.ActionMethod.Name,
                IdentityParamValue = m.ActionResourceIdentityValue,
                Methods = m.HttpMethods
            });
        }
       
        // Private class containing action method details searched to find
        // action methods matching a specific set of conventions.
        private class ActionMethodSearchInfo
        {
            public Type ResourceType { get; set; }
            public bool AcceptsResourceArg { get; set; }
            public bool AcceptsIdentityParam { get; set; }
            public bool ReturnsResourceType { get; set; }
            public string[] HttpMethods { get; set; }
            public MethodInfo ActionMethod { get; set; }
            public ActionParamValue ActionResourceIdentityValue { get; set; }
        }
    }
}
