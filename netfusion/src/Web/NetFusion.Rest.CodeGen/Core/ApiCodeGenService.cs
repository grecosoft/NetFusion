using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.CodeGen.Plugin;

namespace NetFusion.Rest.CodeGen.Core
{
    /// <summary>
    /// Service that looks up a generated type file for a corresponding
    /// resource exposed by the API.  These generated file are created
    /// by the CodeGenModule when the Microservice starts. 
    /// </summary>
    public class ApiCodeGenService : IApiCodeGenService
    {
        private readonly ILogger _logger;
        private readonly ICodeGenModule _codeGenModule;

        public ApiCodeGenService(
            ILogger<ApiCodeGenService> logger,
            ICodeGenModule codeGenModule)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _codeGenModule = codeGenModule ?? throw new ArgumentNullException(nameof(codeGenModule));
        }

        public bool TryGetResourceCodeFile(string resourceName, out Stream stream)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource Name must be specified..", nameof(resourceName));
            
            stream = null;

            string codeGenDir = _codeGenModule.CodeGenConfig.CodeGenerationDirectory;
            string genCodeFilePath = Path.Combine(codeGenDir, resourceName + ".ts");

            if (! File.Exists(genCodeFilePath))
            {
                return false;
            }

            try
            {
                stream = File.OpenRead(genCodeFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading code-generated file.");
                return false;
            }
        }
    }
}
