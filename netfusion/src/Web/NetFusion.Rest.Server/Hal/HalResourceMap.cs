using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Hal.Core;
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
        /// Returns resource meta-data class for a specific model type used to
        /// provided mappings on how the resource should be augmented with 
        /// REST/HAL information.
        /// </summary>
        /// <typeparam name="TModel">The type of model associated with the resource.</typeparam>
        /// <returns>The created resource mapping.</returns>
        protected HalResourceMeta<TModel> Map<TModel>()
            where TModel : class
        {
            var resourceMeta = new HalResourceMeta<TModel>();
            AddResourceMeta(resourceMeta);
            return resourceMeta;
        }
    }
} 
