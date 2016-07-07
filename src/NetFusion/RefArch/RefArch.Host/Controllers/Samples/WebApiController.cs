using NetFusion.WebApi.Metadata;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.webapi", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/webapi")]
    [Authorize]
    public class WebApiController : ApiController
    {

        public WebApiController()
        {

        }
    }
}