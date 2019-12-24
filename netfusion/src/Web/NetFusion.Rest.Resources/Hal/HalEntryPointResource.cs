namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Resource returned from an API service containing Links to root resources at which the client 
    /// can begin navigation.  The links  will often be templates at the root level.
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
    }
}
