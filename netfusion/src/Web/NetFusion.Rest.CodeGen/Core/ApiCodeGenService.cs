using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.CodeGen.Plugin;

namespace NetFusion.Rest.CodeGen.Core
{
    public class ApiCodeGenService : IApiCodeGenService
    {
        private readonly ILogger _logger;
        private readonly ICodeGenModule _codeGenModule;

        public ApiCodeGenService(
            ILogger<ApiCodeGenService> logger,
            ICodeGenModule codeGenModule)
        {
            _logger = logger;
            _codeGenModule = codeGenModule ?? throw new ArgumentNullException(nameof(codeGenModule));
        }

        public bool TryGetResourceCodeFile(string resourceName, out Stream stream)
        {
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
