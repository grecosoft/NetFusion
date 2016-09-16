using NetFusion.Bootstrap.Container;

namespace NetFusion.EntityFramework.Configs
{
    /// <summary>
    /// Container configuration that can be specified by the 
    /// host application.
    /// </summary>
    public class EntityFrameworkConfig : IContainerConfig
    {
        /// <summary>
        /// If true, all of the entity framework type mappings within
        /// the same namespace or child namespace of the context will
        /// be registered with the context.
        /// </summary>
        public bool AutoRegisterTypeMappings { get; set; } = true;
    }
}
