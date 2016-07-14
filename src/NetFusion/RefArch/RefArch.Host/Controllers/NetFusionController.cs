using Microsoft.AspNet.SignalR;
using NetFusion.Bootstrap.Container;
using NetFusion.WebApi.Metadata;
using Newtonsoft.Json;
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
            return Json(AppContainer.Instance.Log, settings);
        }

        [HttpPost Route("composite/log")]
        public void LogCompositConfig(Dictionary<string, object> log) 
        {
            var logHub = GlobalHost.ConnectionManager.GetHubContext<CompositeLogHub>();
            logHub.Clients.All.Log(log);
        }
    }
}