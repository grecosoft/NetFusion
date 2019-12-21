using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Mappings;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Base class from which application specific resource metadata mappings derive.
    /// A resource map determines the HAL specific values such as links to be associated
    /// with one or more resources.
    /// </summary>
    public abstract class HalResourceMap : ResourceMap
    {
        public override string MediaType => InternetMediaTypes.HalJson;

        protected HalResourceMap()
        {
            // Specify the provider responsible for applying the metadata
            // to the returned HAL based resources.
            SetProvider<HalResourceProvider>();
        }

        /// <summary>
        /// Returns meta-data class for a specific resource type used to
        /// provided mappings on how the resource should be augmented with 
        /// REST/HAL information.
        /// </summary>
        /// <typeparam name="TResource">The type of resource to create mapping.</typeparam>
        /// <returns>The mapping for the specified resource.</returns>
        protected HalResourceMeta<TResource> Map<TResource>()
            where TResource : class
        {
            var resourceMeta = new HalResourceMeta<TResource>();
            AddResourceMeta(resourceMeta);
            return resourceMeta;
        }
    }
} 
