using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Allows plug-ins to define specific configurations that can
    /// be initialized by the host application to alter the behavior
    /// of the plug-in.
    /// </summary>
    public interface IContainerConfig : IKnownPluginType
    {
    }
}
