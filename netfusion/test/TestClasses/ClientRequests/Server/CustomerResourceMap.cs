using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Hal;

namespace TestClasses.ClientRequests.Server
{
    /// <summary>
    /// Resource mapping class used by the unit-tests that will be discovered
    /// by the NetFusion container REST/HAL Plug-in.
    /// </summary>
    public class CustomerResourceMap : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            Map<CustomerModel>()
                .LinkMeta<CustomerController>(meta => 
                    meta.Url(RelationTypes.Self, (c, r) => c.GetCustomer(r.CustomerId)));
        }
    }
}
