using NetFusion.Rest.Server.Meta;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// HAL resource metadata for a given resource type.  
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    public class HalResourceMeta<TResource> : ResourceMeta<HalResourceMeta<TResource>, TResource>
        where TResource : class
    {
        // ** Addition needed HAL specific methods to record any metadata can be added.
        // ** This class derives from ResourceMeta which allows adding links to the
        // ** specified resource type. 
    }
}
