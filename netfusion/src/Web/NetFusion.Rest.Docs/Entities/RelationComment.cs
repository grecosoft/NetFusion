namespace NetFusion.Rest.Docs.Entities
{
    /// <summary>
    /// Comments associated with a resource's linked relation.
    /// </summary>
    public class RelationComment
    {
        /// <summary>
        /// The name of the relation specified when adding a link to a resource.
        /// </summary>
        public string RelationName { get; set; }
        
        /// <summary>
        /// The optional resource name identifying the resource containing the named relation.
        /// </summary>
        public string ResourceName { get; set; }
        
        /// <summary>
        /// The comments describing how the relation applies to the resource.
        /// </summary>
        public string Comments { get; set; }
    }
}