namespace NetFusion.Rest.Docs.Models
{
    public class ApiParameterDoc
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object DefaultValue { get; set; }
        public bool IsOptional { get; set; }
        public string Description { get; set; }
    }
}