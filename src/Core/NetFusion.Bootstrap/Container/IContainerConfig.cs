using NetFusion.Base.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Allows plug-ins to define specific configurations that can
    /// be initialized by the host application to alter the behavior
    /// of the container or plug-in.
    /// </summary>
    public interface IContainerConfig : IKnownPluginType
    {
    }
}
