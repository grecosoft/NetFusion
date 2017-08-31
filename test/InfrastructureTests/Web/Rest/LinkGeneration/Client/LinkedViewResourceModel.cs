using NetFusion.Rest.Client.Resources;

namespace InfrastructureTests.Web.Rest.LinkGeneration.Client
{
    public class LinkedViewResourceModel : HalResource
    {
        public int Id { get; set; }
        public int ValueTotal { get; set; }
        public bool ValueOneGreaterValueFour { get; set; }
    }
}
