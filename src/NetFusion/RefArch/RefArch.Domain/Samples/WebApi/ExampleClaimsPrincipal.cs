using NetFusion.Common;
using System.Security.Principal;

namespace RefArch.Domain.Samples.WebApi
{
    public class ExampleClaimsPrincipal : FusionPrincipal
    {
        public ExampleClaimsPrincipal(IPrincipal principal)
            : base(principal)
        {
           
        }

        public string UserId => FindFirst("UserId").Value;
        public string Email => FindFirst("Email").Value;
        public string FirstName => FindFirst("FirstName").Value;
        public string LastName => FindFirst("LastName").Value;
        public string Unit => FindFirst("Unit").Value;
    }
}
