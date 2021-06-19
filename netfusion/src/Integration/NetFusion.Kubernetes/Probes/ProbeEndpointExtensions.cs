using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        /// <returns>Application Builder</returns>
        public static IApplicationBuilder AddStartupProbe(this IApplicationBuilder app,
            string route = "mgt/startup-check",
            int notStartedStatus = StatusCodes.Status503ServiceUnavailable) => AddProbe(app, route, notStartedStatus);

        /// <summary>
        /// Adds route called to determine if the Composite Application has finished is ready to receive requests.
        /// A reference to this URL can be configured as a readiness probe within a Kubernetes pod definition and
        /// will be called after the Startup Probe succeeds or immediately if a startup probe is not specified in
        /// Kubernetes pod definition.  
        /// </summary>
        /// <param name="app">The application builder being configured by the running host</param>
        /// <param name="route">The route at which the action can be called.</param>
        /// <param name="notReadyStatus">The status to return when the service is not ready.</param>
        /// <returns>Application Builder</returns>
        public static IApplicationBuilder AddReadinessProbe(this IApplicationBuilder app,
            string route = "mgt/ready-check",
            int notReadyStatus = StatusCodes.Status503ServiceUnavailable) => AddProbe(app, route, notReadyStatus);

        /// <summary>
        /// Adds route called to determine if the Composite Application is in a healthy state.  A reference
        /// to this URL can be configured as a liveness probe within a Kubernetes pod definition and will be
        /// called to determine the Microservice's health.  
        /// </summary>
        /// <param name="app">The application builder being configured by the running host</param>
        /// <param name="route">The route at which the action can be called.</param>
        /// <param name="notHealthyStatus">The status to return when the service is not healthy.</param>
        /// <returns>Application Builder</returns>
        public static IApplicationBuilder AddHealthProbe(this IApplicationBuilder app,
            string route = "mgt/health-check",
            int notHealthyStatus = StatusCodes.Status503ServiceUnavailable)
        {
            app.UseEndpoints(endpoints =>
            {
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
            });

            return app;
        }
        
        private static IApplicationBuilder AddProbe(IApplicationBuilder app,
            string route,
            int negativeStatus = StatusCodes.Status503ServiceUnavailable)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(route, c =>
                {
                    c.Response.StatusCode = CompositeApp.Instance?.IsReady ?? false
                        ? StatusCodes.Status200OK
                        : negativeStatus;

                    return Task.CompletedTask;
                });
            });

            return app;
        }
    }
}