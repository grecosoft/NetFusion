using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NetFusion.Common.Base.Properties;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Health;

namespace NetFusion.Web.Extensions;

public static class HealthEndpointExtensions
{
    /// <summary>
    /// Adds route called to determine if the Composite Application has finished bootstrapping and is
    /// ready to receive requests.  A reference to this URL can be configured as a startup probe within
    /// a Kubernetes pod definition.  If the Microservice does not start in the allotted time, the
    /// container running the service is re-created.
    /// </summary>
    /// <param name="endpoints">The builder used to configure HTTP endpoints.</param>
    /// <param name="route">The route at which the action can be called.</param>
    /// <param name="notStartedStatus">The status to return when the service is not ready.</param>
    /// <returns>Endpoint Route Builder</returns>
    public static IEndpointRouteBuilder MapStartupCheck(this IEndpointRouteBuilder endpoints,
        string route = "/mgt/startup-check",
        int notStartedStatus = StatusCodes.Status503ServiceUnavailable)
    {
        if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Start Route not Specified", nameof(route));

        var compositeApp = GetCompositeApp(endpoints);
            
        endpoints.MapGet(route, c =>
        {
            c.Response.StatusCode = compositeApp.IsStarted 
                ? StatusCodes.Status200OK
                : notStartedStatus;

            return Task.CompletedTask;
        });

        return endpoints;
    }

    /// <summary>
    /// Adds route called to determine if the Composite Application has finished is ready to receive requests.
    /// A reference to this URL can be configured as a readiness probe within a Kubernetes pod definition and
    /// will be called after the Startup Probe succeeds or immediately if a startup probe is not specified in
    /// Kubernetes pod definition.  
    /// </summary>
    /// <param name="endpoints">The builder used to configure HTTP endpoints.</param>
    /// <param name="route">The route at which the action can be called.</param>
    /// <param name="notReadyStatus">The status to return when the service is not ready.</param>
    /// <returns>Endpoint Route Builder</returns>
    public static IEndpointRouteBuilder MapReadinessCheck(this IEndpointRouteBuilder endpoints,
        string route = "/mgt/ready-check",
        int notReadyStatus = StatusCodes.Status503ServiceUnavailable)
    {
        if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Readiness Check Route not Specified", nameof(route));

        var compositeApp = GetCompositeApp(endpoints);
        
        // Exclude this route from being logged for OK response status codes.
        compositeApp.Properties.AddLogUrlFilter(route, HttpStatusCode.OK);

        endpoints.MapGet(route, c =>
        {
            c.Response.StatusCode = compositeApp.IsReady 
                ? StatusCodes.Status200OK
                : notReadyStatus;

            return Task.CompletedTask;
        });

        return endpoints;
    }

    /// <summary>
    /// Adds route called to determine if the Composite Application is in a healthy state.  A reference
    /// to this URL can be configured as a liveness probe within a Kubernetes pod definition and will be
    /// called to determine the Microservice's health.  
    /// </summary>
    /// <param name="endpoints">The builder used to configure HTTP endpoints.</param>
    /// <param name="route">The route at which the action can be called.</param>
    /// <param name="notHealthyStatus">The status to return when the service is not healthy.</param>
    /// <returns>Endpoint Route Builder</returns>
    public static IEndpointRouteBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints,
        string route = "/mgt/health-check",
        int notHealthyStatus = StatusCodes.Status503ServiceUnavailable)
    {
        if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Health Check Route not Specified", nameof(route));

        var compositeApp = GetCompositeApp(endpoints);
           
        // Exclude this route from being logged for OK response status codes.
        compositeApp.Properties.AddLogUrlFilter(route, HttpStatusCode.OK);
            
        endpoints.MapGet(route, async c =>
        {
            var healthCheck = await compositeApp.GetHealthCheckAsync();
                    
            c.Response.StatusCode = healthCheck.CompositeAppHealth == HealthCheckStatusType.Healthy ?
                StatusCodes.Status200OK : notHealthyStatus;

            c.Response.Headers["Health-Status"] = new StringValues(healthCheck.CompositeAppHealth.ToString());
        });

        return endpoints;
    }

    private static ICompositeApp GetCompositeApp(IEndpointRouteBuilder endpoints)
    {
        var compositeApp =  endpoints.ServiceProvider.GetService<ICompositeApp>();
        if (compositeApp == null)
        {
            throw new InvalidOperationException(
                $"Composite-Application not composed - Call Compose on {typeof(ICompositeContainerBuilder)}");
        }

        return compositeApp;
    }
}