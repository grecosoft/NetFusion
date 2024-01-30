using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using NetFusion.Web.Rest.CodeGen.Plugin;

namespace NetFusion.Web.Rest.CodeGen.Core;

/// <summary>
/// Service that looks up a generated type file for a corresponding
/// resource exposed by the API.  These generated file are created
/// by the CodeGenModule when the Microservice starts. 
/// </summary>
public class ApiCodeGenService(
    ILogger<ApiCodeGenService> logger,
    ICodeGenModule codeGenModule)
    : IApiCodeGenService
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    private readonly ICodeGenModule _codeGenModule = codeGenModule ?? 
        throw new ArgumentNullException(nameof(codeGenModule));

    public bool TryGetResourceCodeFile(string resourceName, [MaybeNullWhen(false)]out Stream stream)
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