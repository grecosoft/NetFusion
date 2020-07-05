namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model describing a named resource embedded within a parent resource.
    /// </summary>
    public class ApiEmbeddedDoc
    {
        /// <summary>
        /// The name further describing what the embedded resource represents.
        /// </summary>
        public string EmbeddedName { get; set; }

        /// <summary>
        /// Documentation associated with the embedded resource.
        /// </summary>
        public ApiResourceDoc ResponseDoc { get; set; } 
    }      
}