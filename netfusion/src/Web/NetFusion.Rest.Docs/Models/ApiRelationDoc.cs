namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model containing a linked relation associated with a resource.
    /// </summary>
    public class ApiRelationDoc
    {
        /// <summary>
        /// The name of the relation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The method used to invoke the action associated with the relation.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The Url used to invoke the action associated with the relation.
        /// </summary>
        public string HRef { get; set; }

        /// <summary>
        /// Description of the relation.
        /// </summary>
        public string Description { get; set; }
    }
}
