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
    }
}
