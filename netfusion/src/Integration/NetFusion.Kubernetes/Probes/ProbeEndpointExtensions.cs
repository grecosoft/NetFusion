using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NetFusion.Base.Properties;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Health;

namespace NetFusion.Kubernetes.Probes
{
    public static class ProbeEndpointExtensions
    {
        /// <summary>
        /// Adds route called to determine if the Composite Application has finished bootstrapping and is
        /// ready to receive requests.  A reference to this URL can be configured as a startup probe within
        /// a Kubernetes pod definition.  If the Microservice does not start in the allotted time, the
        /// container running the service is re-created.
        /// </summary>
        /// <param name="app">The application builder being configured by the running host</param>
        /// <param name="route">The route at which the action can be called.</param>
        /// <param name="notStartedStatus">The status to return when the service is not ready.</param>
        /// <returns>Endpoint Route Builder</returns>
        public static IEndpointRouteBuilder MapStartupProbe(this IEndpointRouteBuilder endpoints,
            string route = "/mgt/startup-check",
            int notStartedStatus = StatusCodes.Status503ServiceUnavailable)
        {
            if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Start Route not Specified", nameof(route));
            
            endpoints.MapGet(route, c =>
            {
                c.Response.StatusCode = CompositeApp.Instance?.IsReady ?? false
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
        /// <param name="app">The application builder being configured by the running host</param>
        /// <param name="route">The route at which the action can be called.</param>
        /// <param name="notReadyStatus">The status to return when the service is not ready.</param>
        /// <returns>Endpoint Route Builder</returns>
        public static IEndpointRouteBuilder MapReadinessProbe(this IEndpointRouteBuilder endpoints,
            string route = "/mgt/ready-check",
            int notReadyStatus = StatusCodes.Status503ServiceUnavailable)
        {
            if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Readiness Check Route not Specified", nameof(route));
            
            CompositeApp.Instance.Properties.AddLogUrlFilter(route, HttpStatusCode.OK);

            endpoints.MapGet(route, c =>
            {
                c.Response.StatusCode = CompositeApp.Instance?.IsReady ?? false
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
        /// <param name="app">The application builder being configured by the running host</param>
        /// <param name="route">The route at which the action can be called.</param>
        /// <param name="notHealthyStatus">The status to return when the service is not healthy.</param>
        /// <returns>Endpoint Route Builder</returns>
        public static IEndpointRouteBuilder MapHealthProbe(this IEndpointRouteBuilder endpoints,
            string route = "/mgt/health-check",
            int notHealthyStatus = StatusCodes.Status503ServiceUnavailable)
        {
            if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));
            
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Health Check Route not Specified", nameof(route));

            CompositeApp.Instance.Properties.AddLogUrlFilter(route, HttpStatusCode.OK);
            
            endpoints.MapGet(route, async c =>
            {
                if (CompositeApp.Instance == null)
                {
                    c.Response.StatusCode = notHealthyStatus;
                    return;
                }
                    
                var healthCheck = await CompositeApp.Instance.GetHealthCheckAsync();
                    
                c.Response.StatusCode = healthCheck.OverallHealth == HealthCheckStatusType.Healthy ?
                    StatusCodes.Status200OK : notHealthyStatus;
            });

            return endpoints;
        }
    }
}