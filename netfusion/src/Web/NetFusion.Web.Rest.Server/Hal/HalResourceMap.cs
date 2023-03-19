using System;
using NetFusion.Web.Common;
using NetFusion.Web.Rest.Server.Hal.Core;
using NetFusion.Web.Rest.Server.Mappings;

namespace NetFusion.Web.Rest.Server.Hal;

/// <summary>
/// Base class from which application specific resource metadata mappings derive.
/// A resource map determines the HAL specific values such as links to be associated
/// with one or more resource models.
/// </summary>
public abstract class HalResourceMap : ResourceMap
{
    public override string MediaType => InternetMediaTypes.HalJson;
    public override Type ProviderType => typeof(HalResourceProvider);
        
    /// <summary>
    /// Returns resource meta-data class for a specific model type used to
    /// provided mappings on how the resource should be augmented with 
    /// REST/HAL information.
    /// </summary>
    /// <typeparam name="TModel">The type of model associated with the resource.</typeparam>
    /// <returns>The created resource metadata.</returns>
    protected HalResourceMeta<TModel> Map<TModel>()
        where TModel : class
    {
        var resourceMeta = new HalResourceMeta<TModel>();
        AddResourceMeta(resourceMeta);
        return resourceMeta;
    }
}