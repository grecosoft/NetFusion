using NetFusion.Common.Extensions;
using RefArch.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace RefArch.Domain.Samples.WebApi
{
    public class PrincipalDependentService :
        IPrincipalDependentService
    {
        private ExampleClaimsPrincipal _principal;
        private IDictionary<string, UserInfo> _userInfo;

        public PrincipalDependentService(ExampleClaimsPrincipal principal)
        {
            _principal = principal;

            
        }

        public UserInfo AccessedPrincipal()
        {
            return _principal.UserInfo;
        }
    }
}
