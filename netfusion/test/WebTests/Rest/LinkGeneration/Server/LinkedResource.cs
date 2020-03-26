namespace WebTests.Rest.LinkGeneration.Server
{
    /// <summary>
    /// Server returned resource used to test server-side link generation.
    /// </summary>
    public class LinkedResource 
    {
        // Resource properties used within resource mappings when
        // specifying resource associated links.
		public int Id { get; set; }
		public int Value1 { get; set; }
		public string Value2 { get; set; }
        public int? Value3 { get; set; }
        public int Value4 { get; set; }
    }
}
