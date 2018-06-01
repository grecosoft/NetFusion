namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Applies the resource metadata to a resource for the given media-type.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Uses the cached REST metadata and applies it to a resource instance.
        /// </summary>
        /// <param name="context">Contains the resource and it associated metadata.
        /// Plus any needed services.</param>
        void ApplyResourceMeta(ResourceContext context);
    }
}
