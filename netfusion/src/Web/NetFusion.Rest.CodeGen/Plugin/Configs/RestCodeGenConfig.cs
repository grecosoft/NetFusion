using System;
using System.IO;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.CodeGen.Plugin.Configs
{
    public class RestCodeGenConfig : IPluginConfig
    {
        /// <summary>
        /// The default directory to which code corresponding to models exposed
        /// by the controllers is generated and read.  
        /// </summary>
        public string CodeGenerationDirectory { get; private set; } = Path.Combine(
            AppContext.BaseDirectory, "ts-resources");

        /// <summary>
        /// Indicates that code generation is disabled. 
        /// </summary>
        public bool IsGenerationDisabled { get; private set; } = false;

        public string EndpointUrl { get; private set; } = "/api/net-fusion/rest";

        /// <summary>
        /// Specifies the directory to which code will be generated.
        /// </summary>
        /// <param name="directory">The path to the directory.</param>
        /// <returns>Configuration</returns>
        public RestCodeGenConfig SetDescriptionDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Directory must be specified", nameof(directory));
            }

            CodeGenerationDirectory = directory;
            return this;
        }

        /// <summary>
        /// Indicates that code generation is disabled.  This can be used
        /// to disable code generation if the public API is not changing
        /// during development.
        /// </summary>
        /// <returns>Configuration</returns>
        public RestCodeGenConfig Disable()
        {
            IsGenerationDisabled = true;
            return this;
        }

    }
}
