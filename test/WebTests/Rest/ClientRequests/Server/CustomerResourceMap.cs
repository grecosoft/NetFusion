using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Hal;

namespace InfrastructureTests.Web.Rest.ClientRequests.Server
{
    /// <summary>
    /// Resource mapping class used by the unit-tests that will be discovered
    /// by the NetFusion container REST/HAL Plug-in.
    /// </summary>
    public class CustomerResourceMap : HalResourceMap
    {
        public override void OnBuildResourceMap()
        {
            Map<CustomerResource>()
                .LinkMeta<CustomerController>(meta => 
                    meta.Url(RelationTypes.Self, (c, r) => c.GetCustomer(r.CustomerId)));
        }
    }
}
