using NetFusion.WebApi.Metadata;
using RefArch.Api.Models;
using RefArch.Domain.Samples.WebApi;
using System.Collections.Generic;
using System.Web.Http;

namespace RefArch.Services
{
    [EndpointMetadata(EndpointName = "RefArch.UserInfo.Api", IncluedAllRoutes = true)]
    [RoutePrefix("api/user/info")]
    public class UserInfoController : ApiController
    {
        private readonly IUserService _userInfoSrv;

        public UserInfoController(IUserService userInfoSrv)
        {
            _userInfoSrv = userInfoSrv;
        }

        [HttpGet, Route("all", Name = "GetAllUserInfo")]
        public IDictionary<string, UserInfo> GetAllUserInfo()
        {
            return _userInfoSrv.ListUserInfo();
        }
    }
}
