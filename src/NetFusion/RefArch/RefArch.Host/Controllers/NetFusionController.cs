using Microsoft.AspNet.SignalR;
using NetFusion.Bootstrap.Container;
using NetFusion.Logging;
using NetFusion.WebApi.Metadata;
using Newtonsoft.Json;
using System.Linq;
using RefArch.Host.Hubs;
using System.Collections.Generic;
using System.Web.Http;

namespace Samples.WebHost.Controllers
{
    [EndpointMetadata(EndpointName = "NetFusion", IncluedAllRoutes = true)]
    [RoutePrefix("api/config")]
    public class WebApiSampleController : ApiController
    {
        private readonly IRouteMetadataService _routeMetadataSrv;

        public WebApiSampleController(IRouteMetadataService routeMetadataSrv)
        {
            _routeMetadataSrv = routeMetadataSrv;
        }

        /// <summary>
        /// Exposes all the controller endpoints as meta-data to the client.  A client side JS proxy
        /// can be written to use this meta-data to make requests without having hard-coded URLs.  In
        /// this case, all of the routes for the examples are exposed.
        /// </summary>
        [AllowAnonymous, HttpGet, Route("routes")]
        [RouteMetadata(IncludeRoute = false)]
        public IDictionary<string, EndpointMetadata> GetRoutes()
        {
            return _routeMetadataSrv.GetApiMetadata();
        }

        [HttpGet Route("composite/log")]
        public IHttpActionResult GetCompositeConfig()
        {
            var settings = new JsonSerializerSettings { };
            var composite = (IComposite)AppContainer.Instance;

            var plugins = composite.Plugins
                .Select(p => new PluginInfo(p))
                .OrderBy(pi => pi.Name);

            var hostLog = new HostLog(
                new CompositeInfo(plugins),
                AppContainer.Instance.Log);

            return Json(hostLog, settings);
        }

        /// <summary>
        /// Called by another Host client to broadcast the configuration to all connected
        /// clients.
        /// </summary>
        /// <param name="hostLog">The host configuration,</param>
        [HttpPost Route("composite/log")]
        public void LogCompositConfig(HostLog hostLog) 
        {
            var logHub = GlobalHost.ConnectionManager.GetHubContext<CompositeLogHub>();
            logHub.Clients.All.Log(hostLog);
        }
    }
}