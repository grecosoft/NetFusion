using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace NetFusion.Rest.Server.Modules
{
    /// <summary>
    /// Contains settings based on the current execution environment.
    /// </summary>
    public class EnvironmentSettings
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRestModule _restModule;

        public EnvironmentSettings(
            IHostingEnvironment hostingEnvironment,
            IRestModule restModule)
        {
            _hostingEnvironment = hostingEnvironment;
            _restModule = restModule;
        }

        /// <summary>
        /// The directory containing the controller XML documents.
        /// </summary>
        /// <returns>Child directory of the content root.</returns>
        public string GetControllerDocPath()
        {
            return Path.Combine(_hostingEnvironment.ContentRootPath, _restModule.GetControllerDocDirectoryName());
        }

        /// <summary>
        /// The optional directory containing type-script definitions.
        /// </summary>
        /// <returns>Child directory of the content root.</returns>
        public string GetTypeScriptPath()
        {
            return Path.Combine(_hostingEnvironment.ContentRootPath, _restModule.GetTypeScriptDirectoryName());
        }
    }
}
