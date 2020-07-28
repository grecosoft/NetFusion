namespace WebTests.Rest.LinkGeneration.Server
{
    /// <summary>
    /// Server model returned as a resource to test server-side link generation.
    /// </summary>
    public class StateModel 
    {
        // Model properties used within mappings when specifying resource associated links.
		public int Id { get; set; }
		public int Value1 { get; set; }
		public string Value2 { get; set; }
        public int? Value3 { get; set; }
        public int Value4 { get; set; }
    }
}
