using NetFusion.Rest.Server.Mappings;
// ReSharper disable RedundantOverriddenMember

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Adds and updates a HAL define resource instance with HAL specific attributes.
    /// </summary>
    public class HalResourceProvider : ResourceProvider
    {
        public override void ApplyResourceMeta(ResourceContext context)
        {
            // Call base provider to add resource links.
            base.ApplyResourceMeta(context);   
           
            // Complete any HAL Resource specific resource updates.
        }
    }
}