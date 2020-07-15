namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model containing a description of a resource's property.
    /// </summary>
    public class ApiPropertyDoc
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the property.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates that the type of property is an array.
        /// </summary>
        public bool IsArray { get; set; }

        /// <summary>
        /// Indicates that a value for the property is required.
        /// </summary>
        public bool IsRequired { get; set; }
        
        public object Type { get; set; }

        public ApiResourceDoc ResourceDoc { get; set; }
    }
}