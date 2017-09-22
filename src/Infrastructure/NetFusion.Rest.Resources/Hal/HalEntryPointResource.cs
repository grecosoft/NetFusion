namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Resource return from an API service containing Links to root resources at which the client 
    /// can begin navigation.  The links  will often be templates at the root level.
    /// </summary>
    public class HalEntryPointResource : HalResource
    {
        /// <summary>
        /// Optional version indicator.
        /// </summary>
        public string Version { get; set; }    
        
        /// <summary>
        /// List of secondary services to which the client requesting
        /// the entry point address can connect.
        /// </summary>
        public HalSecondaryService[] SecondaryServices { get; set; }
    }
}
