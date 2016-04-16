using NetFusion.Bootstrap.Container;

namespace NetFusion.EntityFramework.Configs
{
    public class EntityFrameworkConfig : IContainerConfig
    {
        public bool AutoRegisterTypeMappings { get; set; } = true;
    }
}
