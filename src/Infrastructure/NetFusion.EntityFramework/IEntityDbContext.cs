using NetFusion.Base.Plugins;

namespace NetFusion.EntityFramework
{
    /// <summary>
    /// Marker interface used to identity entity database
    /// context classes that are wrapped by the EntityContext.
    /// </summary>
    public interface IEntityDbContext : IKnownPluginType
    {
        
    }
}
