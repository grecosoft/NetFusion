using NetFusion.Messaging;
using NetFusion.WebApi;
using NetFusion.WebApi.Metadata;
using RefArch.Api.Commands;
using RefArch.Api.Models;
using RefArch.Domain.Samples.WebApi;
using RefArch.Host.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.webapi", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/webapi")]
    [Authorize]
    public class WebApiController : ApiController
    {
        private readonly IMessagingService _messagingSrv;
        private readonly IJwtTokenService _jwtTokenService;

        public WebApiController(
            IMessagingService messagingSrv,
            IJwtTokenService jwtTokenSrv)
        {
            _messagingSrv = messagingSrv;
            _jwtTokenService = jwtTokenSrv;
        }

        [AllowAnonymous, HttpPost, Route("login", Name = "LoginUser")]
        public async Task<HttpResponseMessage> LoginUser(AuthorizeUserCommand authorize)
        {
            UserLoginInfo result = await _messagingSrv.PublishAsync(authorize);
            if (result == UserLoginInfo.InvalidUser)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Login");
            }

            var token = _jwtTokenService.CreateSecurityToken(result);
            return this.Request.Authenticated(result, token);
        }
    }
}