using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.Server.Plugin
{
    /// <summary>
    /// Configurations associated with the core REST/HAL implementation.
    /// </summary>
    public interface IRestModule : IPluginModuleService
    {
        /// <summary>
        /// Returns the suffix used for API controllers.
        /// </summary>
        /// <returns>The controller suffix.</returns>
        string GetControllerSuffix();       
    }
}
