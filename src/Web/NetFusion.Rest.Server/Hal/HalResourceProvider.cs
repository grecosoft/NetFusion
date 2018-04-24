using NetFusion.Rest.Server.Mappings;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Adds and updates a HAL define resource instance with HAL specific attributes.
    /// </summary>
    public class HalResourceProvider : ResourceProvider
    {
        public override void ApplyResourceMeta(ResourceContext context)
        {
            base.ApplyResourceMeta(context);    // Call base provider to add resource links.
           
            // Complete any HAL Resource specific resource updates.
        }
    }
}