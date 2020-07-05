namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Documentation model for the different parameter inputs
    /// accepted by a Web Api method.
    /// </summary>
    public class ApiParameterDoc
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The JSON type of the parameter.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The default value of the parameter if not specified.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Indicates if the parameter is optional or must be specified.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Description of the parameter.
        /// </summary>
        public string Description { get; set; }
    }
}