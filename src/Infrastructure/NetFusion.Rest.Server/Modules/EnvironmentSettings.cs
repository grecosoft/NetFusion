using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace NetFusion.Rest.Server.Modules
{
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

        public string GetControllerDocPath()
        {
            return Path.Combine(_hostingEnvironment.ContentRootPath, _restModule.GetControllerDocDirectoryName());
        }

        public string GetTypeScriptPath()
        {
            return Path.Combine(_hostingEnvironment.ContentRootPath, _restModule.GetTypeScriptDirectoryName());
        }
    }
}
