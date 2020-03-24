 namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Model containing information about the Api.
    /// </summary>
    public class EntryPointModel
    {
        /// <summary>
        /// Value indicating the version of the API.
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        /// Optional URL to document describing the API.  Since most developers
        /// don't document, this will most likely be null or empty-string.
        /// </summary>
        public string ApiDocUrl { get; set; }
    }    
}