namespace NetFusion.WebApi.Metadata
{
    /// <summary>
    /// Metadata about a parameter that this found within a given
    /// route template.
    /// </summary>
    public class ParameterMetadata
    {
        public string Name { get; set; }
        public bool IsOptional { get; set; }
        public string Type { get; set; }
        public object DefaultValue { get; set; }
    }
}
