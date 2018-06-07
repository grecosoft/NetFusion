using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Action link for which the URI is expressed as an interpolated string using
    /// property values of the resource to substitute URI values at runtime.  Under
    /// the hood, an interpolated string is a call to the String.Format method.
    /// </summary>
    public abstract class ActionResourceLink : ActionLink
    {
        /// <summary>
        /// The string containing the formatted URL.
        /// </summary>
        public string FormattedUrl { get; protected set; }

        /// <summary>
        /// The resource properties contained within the format expression.
        /// </summary>
        public PropertyInfo[] FormattedResourceProps { get; protected set; }

        /// <summary>
        /// Returns the populated format string using the state of the specified resource.
        /// </summary>
        /// <param name="resource">The resource used to populate the formatted string.</param>
        /// <returns>The formatted string.</returns>
        public abstract string FormatUrl(IResource resource);
    }

    /// <summary>
    /// String value that is interpolated with resource property values.
    /// </summary>
    /// <typeparam name="TResource">The type of resource being interpolated.</typeparam>
    public class ActionResourceLink<TResource> : ActionResourceLink
        where TResource : class, IResource
    {
        /// <summary>
        /// The function delegate used to invoke the string interpolation specified at compile time.
        /// </summary>
        public Func<TResource, string> ResourceUrlFormatFunc { get; }

        public ActionResourceLink(Expression<Func<TResource, string>> resourceUrl)
        {
            if (resourceUrl == null)throw new ArgumentNullException(nameof(resourceUrl),
                "Resource URL cannot be null.");

            // Compile the expression so it can be executed at runtime against a resource.
            ResourceUrlFormatFunc = resourceUrl.Compile();

            var formatUrlCallExp = (MethodCallExpression)resourceUrl.Body;
            SetUrlFormatString(formatUrlCallExp);
            SetUrlFormattedValues(formatUrlCallExp);
        }

        private void SetUrlFormatString(MethodCallExpression formatUrlCallExp)
        {
            if (!formatUrlCallExp.Arguments.Any())
            {
                throw new InvalidOperationException("Expression does not contain formatted string.");
            }

            // The first argument of the format call expression is the format string.
            var formatUrlConstant = (ConstantExpression)formatUrlCallExp.Arguments[0];
            FormattedUrl = (string)formatUrlConstant.Value;
        }

        // Obtains the PropertyInfo for all resource properties used in the format string.
        private void SetUrlFormattedValues(MethodCallExpression formatUrlCallExp)
        {
            var resourceProps = new List<PropertyInfo>();

            AddExpressionArgs(formatUrlCallExp.Arguments, resourceProps);

            // Checks for the case where the arguments to be formatted are specified as an array.
            if (! resourceProps.Any() && formatUrlCallExp.Arguments.Count() == 2)
            {
                if (formatUrlCallExp.Arguments[1] is NewArrayExpression arrayExp)
                {
                    AddExpressionArgs(arrayExp.Expressions, resourceProps);
                }
            }
           
            FormattedResourceProps = resourceProps.ToArray();
        }

        private static void AddExpressionArgs(IReadOnlyList<Expression> arguments, ICollection<PropertyInfo> resourceProps)
        {
            for (var i = 0; i < arguments.Count(); i++)
            {
                var memberExp = arguments[i] as MemberExpression;

                if (memberExp?.Member is PropertyInfo propInfo)
                {
                    resourceProps.Add(propInfo);
                    continue;
                }

                // This is the case if the argument containing the resource property cast to an object.
                var callExp = arguments[i] as UnaryExpression;
                var propExp = callExp?.Operand as MemberExpression;
                propInfo = propExp?.Member as PropertyInfo;

                if (propInfo != null)
                {
                    resourceProps.Add(propInfo);
                }
            }
        }

        /// <summary>
        /// Executes the function corresponding to a formatted string using the state of the
        /// passed resource.
        /// </summary>
        /// <param name="resource">The resource used to format string.</param>
        /// <returns>The formatted string populated with resource property values.</returns>
        public override string FormatUrl(IResource resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource), 
                "Resource cannot be null.");

            TResource typedResource = (TResource)resource;
            return ResourceUrlFormatFunc(typedResource);
        }
    }
}
