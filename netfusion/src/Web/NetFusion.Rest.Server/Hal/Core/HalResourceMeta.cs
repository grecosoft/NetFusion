using NetFusion.Rest.Server.Meta;

namespace NetFusion.Rest.Server.Hal.Core
{
    /// <summary>
    /// HAL resource metadata for a given model type.  
    /// </summary>
    /// <typeparam name="TModel">The type of model associated with resource.</typeparam>
    public class HalResourceMeta<TModel> : ResourceMeta<HalResourceMeta<TModel>, TModel>
        where TModel : class
    {
        // ** Addition needed HAL specific methods to record any metadata can be added.
        // ** This class derives from ResourceMeta which allows adding links to the
        // ** specified resource type. 
    }
}
