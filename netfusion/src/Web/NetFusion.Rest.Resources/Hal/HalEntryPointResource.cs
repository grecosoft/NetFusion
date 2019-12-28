namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Resource returned from an API service containing Links to root resources
    /// the client uses to begin navigation.  The links  will often be templates
    /// at the root level.  After the root resources are loaded, the links directly
    /// related with subsequent the resources are usually used.
    /// </summary>
    public class HalEntryPointResource : HalResource<EntryPointModel>
    {
        public HalEntryPointResource(EntryPointModel model): base(model)
        {
            
        }
    }
    
    /// <summary>
    /// Model containing information about the Api.
    /// </summary>
    public class EntryPointModel
    {
        public string Version { get; set; }
        public string ApiDocUrl { get; set; }
    }
}
