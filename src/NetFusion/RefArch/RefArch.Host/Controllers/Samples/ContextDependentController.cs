using NetFusion.WebApi.Metadata;
using RefArch.Api.Models;
using RefArch.Domain.Samples.WebApi;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.webapi.context", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/webapi/authorized")]
    [Authorize]
    public class ContextDependentController : ApiController
    {
        private readonly IPrincipalDependentService _principalDependentSrv;

        public ContextDependentController(IPrincipalDependentService principalDependentSrv)
        {
            _principalDependentSrv = principalDependentSrv;
        }

        [HttpGet, Route("context", Name = "GetAuthorizedContext")]
        public UserInfo GetAuthorizedContext()
        {
            return _principalDependentSrv.AccessedPrincipal();
        }
    }
}