using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Meta;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// HAL resource metadata for a given resource type.  
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    public class HalResourceMeta<TResource> : ResourceMeta<HalResourceMeta<TResource>, TResource>
        where TResource : class, IHalResource
    {       
        public HalResourceMeta(IResourceMap resourceMap) : base(resourceMap)
        {

        }

        // ** Addition needed HAL specific methods to record any metadata can be added.
        // ** Currently, this class derives from ResourceMeta which allows adding links
        // ** to the specified resource type. 
    }
}
