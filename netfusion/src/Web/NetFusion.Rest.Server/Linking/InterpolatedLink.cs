using System;
using System.Linq.Expressions;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// Resource link for which the URI is expressed as an interpolated string using
    /// property values of the resource to substitute URI values at runtime.
    /// 
    /// Example = $"http://services.customer/{resource.CustomerId}"
    /// </summary>
    public abstract class InterpolatedLink : ResourceLink
    {
        /// <summary>
        /// Returns the populated format string using the state of the specified resource.
        /// </summary>
        /// <param name="resource">The resource used to populate the formatted string.</param>
        /// <returns>The formatted string.</returns>
        public abstract string FormatUrl(object resource);
    }

    /// <summary>
    /// String value that is interpolated with resource property values.
    /// </summary>
    /// <typeparam name="TResource">The type of resource being interpolated.</typeparam>
    public class InterpolatedLink<TResource> : InterpolatedLink
        where TResource : class
    {
        /// <summary>
        /// The function delegate used to invoke the string interpolation specified at compile time.
        /// </summary>
        public Func<TResource, string> ResourceUrlFormatFunc { get; }

        public InterpolatedLink(Expression<Func<TResource, string>> resourceUrl)
        {
            if (resourceUrl == null)throw new ArgumentNullException(nameof(resourceUrl),
                "Resource URL cannot be null.");

            // Compile the expression so it can be executed at runtime against a resource.
            ResourceUrlFormatFunc = resourceUrl.Compile();
        }

        /// <summary>
        /// Executes the function corresponding to a formatted string using the state of the
        /// passed resource.
        /// </summary>
        /// <param name="resource">The resource used to format string.</param>
        /// <returns>The formatted string populated with resource property values.</returns>
        public override string FormatUrl(object resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource), 
                "Resource cannot be null.");

            TResource typedResource = (TResource)resource;
            return ResourceUrlFormatFunc(typedResource);
        }
    }
}
