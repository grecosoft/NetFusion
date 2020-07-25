namespace NetFusion.Rest.Docs.Entities
{
    /// <summary>
    /// Comments associated with an embedded resource.
    /// </summary>
    public class EmbeddedComment
    {
        /// <summary>
        /// The embedded name matching the name specified by the EmbeddedResource Attribute.
        /// </summary>
        public string EmbeddedName { get; set; }
        
        /// <summary>
        /// The optional parent resource name, if present, matching the name specified
        /// by the EmbeddedResource Attribute.
        /// </summary>
        public string ParentResourceName { get; set; }
        
        /// <summary>
        /// The optional child resource name, if present, matching the name specified
        /// by the EmbeddedResource Attribute.
        /// </summary>
        public string ChildResourceName { get; set; }
        
        /// <summary>
        /// Comment describing how the embedded child resource relates to the parent resource.  
        /// </summary>
        public string Comments { get; set; }
    }
}