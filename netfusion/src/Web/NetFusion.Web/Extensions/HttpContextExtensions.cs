using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Web.Extensions;

/// <summary>
/// HTTP Context extensions for use within MVC based classes such as formatters.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Returns a service registered with the dependency injection container from
    /// the context of a Web MVC component class.
    /// </summary>
    /// <typeparam name="T">The type of the service to receive.</typeparam>
    /// <param name="httpContext">The httpContext of the current request.</param>
    /// <returns>The registered service.</returns>
    public static T GetService<T>(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        return (T)httpContext.RequestServices.GetRequiredService(typeof(T));
    }
}