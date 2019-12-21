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
        /// Returns the populated format string using the resource's model.
        /// </summary>
        /// <param name="model">The model used to populate the formatted string.</param>
        /// <returns>The formatted string.</returns>
        public abstract string FormatUrl(object model);
    }

    /// <summary>
    /// String value that is interpolated with a resource model's property values.
    /// </summary>
    /// <typeparam name="TModel">The type of resource being interpolated.</typeparam>
    public class InterpolatedLink<TModel> : InterpolatedLink
        where TModel : class
    {
        /// <summary>
        /// The function delegate used to invoke the string interpolation specified at compile time.
        /// </summary>
        public Func<TModel, string> ResourceUrlFormatFunc { get; }

        public InterpolatedLink(Expression<Func<TModel, string>> resourceUrl)
        {
            if (resourceUrl == null)throw new ArgumentNullException(nameof(resourceUrl),
                "Resource URL cannot be null.");

            // Compile the expression so it can be executed at runtime against a resource.
            ResourceUrlFormatFunc = resourceUrl.Compile();
        }

        /// <summary>
        /// Executes the function corresponding to a formatted string using the state
        /// of the passed model.
        /// </summary>
        /// <param name="model">The resource's model used to format string.</param>
        /// <returns>The formatted string populated with model's property values.</returns>
        public override string FormatUrl(object model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model), 
                "Model cannot be null.");

            TModel typedModel = (TModel)model;
            return ResourceUrlFormatFunc(typedModel);
        }
    }
}
