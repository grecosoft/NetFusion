using NetFusion.WebApi.Metadata;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.webapi", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/webapi")]
    public class WebApiController : ApiController
    {

    }
}