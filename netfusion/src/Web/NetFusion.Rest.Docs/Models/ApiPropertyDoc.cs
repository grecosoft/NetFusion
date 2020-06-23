namespace NetFusion.Rest.Docs.Models
{
    public class ApiPropertyDoc
    {
        public bool IsArray { get; set; }
        public string Name { get; set; }
        
        public bool IsRequired { get; set; }
        
        // Will be string containing the json type, or if a complex type, an
        // instance of ApiResourceDoc.  Not using common base class since this
        // is a model and want MS serializer to serialize derived types.
        public object Type { get; set; }         
        public string Description { get; set; }
    }
}