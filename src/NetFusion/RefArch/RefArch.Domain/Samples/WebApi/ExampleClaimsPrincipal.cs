using NetFusion.Common;
using RefArch.Api.Models;
using System.Security.Principal;

namespace RefArch.Domain.Samples.WebApi
{
    public class ExampleClaimsPrincipal : FusionPrincipal
    {
        public UserInfo UserInfo { get; private set; }

        public ExampleClaimsPrincipal(IPrincipal principal)
            : base(principal) { }

        public string UserId => FindFirst("UserId").Value;
        public string Email => FindFirst("Email").Value;
        public string FirstName => FindFirst("FirstName").Value;
        public string LastName => FindFirst("LastName").Value;
        
        public void SetUserContextInfo(UserInfo userInfo)
        {
            this.UserInfo = userInfo;
        }
    }
}
