using Microsoft.AspNetCore.Builder;
using NetFusion.Rest.Docs.Core;

namespace NetFusion.Rest.Docs.Plugin
{
    /// <summary>
    /// Application builder configuration extensions.
    /// </summary>
    public static class RestDocExtensions
    {
        /// <summary>
        /// Adds the middleware exposing an endpoint used to query
        /// WebApi documentation.
        /// </summary>
        /// <param name="builder">The builder to be configured.</param  >
        /// <returns>Reference to the builder.</returns>
        public static IApplicationBuilder UseRestDocs(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.UseMiddleware<ApiDocMiddleware>();
            return builder;
        }
    }
}