using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NetFusion.Bootstrap.Container;

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
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="route"></param>
        /// <param name="notHealthyStatus"></param>
        /// <returns></returns>
        public static IApplicationBuilder AddLivenessProbe(this IApplicationBuilder app,
            string route = "mgt/liveness-check",
            int notHealthyStatus = StatusCodes.Status503ServiceUnavailable) => AddProbe(app, route, notHealthyStatus);
        
        /// <summary>
        /// Adds a route when called will toggle the current readiness status of the service's
        /// associated Composite Application.
        /// </summary>
        /// <param name="app">The application builder being configured by the running host</param>
        /// <param name="route">The route at which the action can be called.</param>
        /// <returns>Application Builder</returns>
        public static IApplicationBuilder AddReadinessToggle(this IApplicationBuilder app,
            string route = "mgt/ready-toggle")
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost(route, c =>
                {
                    string updatedStatus = CompositeApp.Instance.ToggleReadyStatus();
                    c.Response.StatusCode = StatusCodes.Status200OK;
                    return c.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(updatedStatus)).AsTask();
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
                    c.Response.StatusCode = (CompositeApp.Instance?.IsReady ?? false)
                        ? StatusCodes.Status200OK
                        : negativeStatus;

                    return Task.CompletedTask;
                });
            });

            return app;
        }
        
        
    }
}