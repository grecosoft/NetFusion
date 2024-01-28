using System;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NetFusion.Common.Base;
using NetFusion.Core.Bootstrap.Container;

namespace NetFusion.Web.Extensions;

public static class LogEndpointExtensions
{
    /// <summary>
    /// Adds route when called will return a JSON document containing a detailed log of
    /// how the Composite Application has been bootstrapped from plugins.
    /// </summary>
    /// <param name="endpoints">The endpoint builder.</param>
    /// <param name="route">The route at which the endpoint can be called.</param>
    /// <returns>Endpoint Route Builder</returns>
    public static IEndpointRouteBuilder MapCompositeLog(this IEndpointRouteBuilder endpoints,
        string route = "/mgt/composite/log")
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Composite Log Query URL not specified.", nameof(route));

        endpoints.MapControllers();
        endpoints.MapGet(route, c =>
        {
            c.Response.ContentType = ContentTypes.Json;
            c.Response.StatusCode = StatusCodes.Status200OK;
            return c.Response.Body
                .WriteAsync(JsonSerializer.SerializeToUtf8Bytes(CompositeApp.Instance.Log))
                .AsTask();
        });
            
        return endpoints;
    }
}