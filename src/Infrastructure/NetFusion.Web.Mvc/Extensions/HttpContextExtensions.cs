using Microsoft.AspNetCore.Http;
using System;

namespace NetFusion.Web.Mvc.Extensions
{
    /// <summary>
    /// HTTP Context extensions for use within MVC based classes such as formatters.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns a service registered with the dependency injection container from
        /// with the context of a Web MVC component class.
        /// </summary>
        /// <typeparam name="T">The type of the service to receive.</typeparam>
        /// <param name="httpContext">The httpContext of the current request.</param>
        /// <returns>The registered service.</returns>
        public static T GetService<T>(this HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            return (T)httpContext.RequestServices.GetService(typeof(T));
        }
    }
}
