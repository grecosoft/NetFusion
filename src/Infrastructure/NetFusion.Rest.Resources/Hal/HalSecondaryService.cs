namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Secondary service associated with the API entry point resource to which consuming
    /// applications can connect to receive the service's associated endpoint resource.
    /// NOTE:  This would be the case if the client was connecting to multiple services
    /// and not using an API Gateway.
    /// </summary>
    public class HalSecondaryService
    {
        /// <summary>
        /// The name used to reference the service.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The base address of the API service.
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// The child path of the base address to load the
        /// service's entry point resource.
        /// </summary>
        public string EntryPointPath { get; set; }
    }
}
