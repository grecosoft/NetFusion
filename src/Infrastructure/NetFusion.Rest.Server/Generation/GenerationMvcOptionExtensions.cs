using System;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Server.Generation.Core;

namespace NetFusion.Rest.Server.Generation
{
    /// <summary>
    /// Extensions for configuring MVC Options.
    /// </summary>
    public static class GenerationMvcOptionExtensions
    {
        /// <summary>
        /// Adds a filter that checks for the presence of a query string parameter
        /// named "rtsd" with the value of "true".  If found, the controller method
        /// is not invoked but the type-script definitions used by the action method
        /// are returned.
        /// </summary>
        /// <param name="options">MVC Options object.</param>
        /// <returns>MVC Options object.</returns>
        public static MvcOptions AllowTypeScriptQueries(this MvcOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "MVC Options not specified.");

            options.Filters.Add(typeof(ResourceTypeFilter), 0);
            return options;
        }
    }
}
