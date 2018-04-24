namespace NetFusion.Rest.Client.Resources
{
    /// <summary>
    /// The response type returned from a service API entry point URL.
    /// This resource will contain URLs (usually template URLs) that
    /// can be used to load initial resources that can be navigated.
    /// </summary>
    public class HalEntryPointResource : HalResource
    {
        /// <summary>
        /// Optional value indicating the version of the service API.
        /// </summary>
        public string Version { get; set; }
    }
}
