using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.Server.Modules
{
    public interface IRestModule : IPluginModuleService
    {
        /// <summary>
        /// Returns the suffix used for API controllers.
        /// </summary>
        /// <returns>The controller suffix.</returns>
        string GetControllerSuffix();

        string GetTypeScriptDirectoryName();

        string GetControllerDocDirectoryName();
    }
}
