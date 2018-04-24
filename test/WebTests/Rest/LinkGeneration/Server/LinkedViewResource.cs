using NetFusion.Rest.Resources.Hal;

namespace InfrastructureTests.Web.Rest.LinkGeneration.Server
{
    public class LinkedViewResource : HalResource
    {
        // For testing want to test all the link scenarios to the view resource
        // will define all the same properties as its base resource.  But this
        // is not a requirement.  The properties on which the links are based
        // must be common between the view and base resource types.
        public int Id { get; set; }
        public int Value1 { get; set; }
        public string Value2 { get; set; }
        public int? Value3 { get; set; }
        public int Value4 { get; set; }

        // View Specific Properties:
        public int ValueTotal { get; }
        public bool ValueOneGreaterValueFour { get; }

        public LinkedViewResource(LinkedResource resource)
        {
            Id = resource.Id;
            Value1 = resource.Value1;
            Value2 = resource.Value2;
            Value3 = resource.Value3;
            Value4 = resource.Value4;

            ValueTotal = resource.Value3 ?? 0 + resource.Value4;
            ValueOneGreaterValueFour = resource.Value1 > resource.Value4;
        }
    }
}
