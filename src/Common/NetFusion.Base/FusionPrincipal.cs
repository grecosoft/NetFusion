using NetFusion.Common;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace NetFusion.Base
{
    /// <summary>
    /// Base principal class from which the application host can derived from to
    /// add specific properties.  
    /// </summary>
    public abstract class FusionPrincipal : ClaimsPrincipal
    {
        private readonly List<string> _roleKeys = new List<string>();

        public FusionPrincipal(IPrincipal principal) : base(principal)
        {
            Check.NotNull(principal, nameof(principal));

            if (principal.Identity.IsAuthenticated)
            {
                //var roleKeys = base.FindFirst(ClaimTypes.Role).Value;
                //if (roleKeys != null)
                //{
                //    _roleKeys.AddRange(roleKeys.Split(','));
                //}
            }
        }

        public string[] Roles { get { return _roleKeys.ToArray(); } }

        public override bool IsInRole(string role)
        {
            return _roleKeys.Contains(role);
        }
    }
}

