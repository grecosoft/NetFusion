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

        /// <summary>
        /// The directory containing XML documents with controller action documentation.
        /// </summary>
        /// <returns></returns>
        string GetControllerDocDirectoryName();

        /// <summary>
        /// The directory containing TypeScript definitions for returned resources.
        /// </summary>
        /// <returns>Directory Name.</returns>
        string GetTypeScriptDirectoryName();        
    }
}
