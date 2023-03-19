using System;
using System.IO;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Web.Rest.CodeGen.Plugin.Configs;

/// <summary>
/// Plugin configuration used to specify settings for code generation.
/// </summary>
public class RestCodeGenConfig : IPluginConfig
{
    /// <summary>
    /// The default directory to which the code is generated.  
    /// </summary>
    public string CodeGenerationDirectory { get; private set; } = Path.Combine(
        AppContext.BaseDirectory, "ts-resources");

    /// <summary>
    /// Indicates that code generation is disabled. 
    /// </summary>
    public bool IsGenerationDisabled { get; private set; }

    /// <summary>
    /// The endpoint called to retrieve generated code.
    /// </summary>
    public string EndpointUrl { get; private set; } = "/api/net-fusion/rest";

    /// <summary>
    /// Specifies the directory to which code will be generated.
    /// </summary>
    /// <param name="directory">The path to the directory.</param>
    /// <returns>Configuration</returns>
    public RestCodeGenConfig SetCodeGenerationDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory must be specified", nameof(directory));
        }

        CodeGenerationDirectory = directory;
        return this;
    }

    /// <summary>
    /// Indicates that code generation is disabled.  This can be used to disable code generation
    /// if the public API is not changing during development or generated and included within the
    /// deployed output.  NOTE:  If code generation is not desired during Microservice bootstrapping,
    /// the TypeGen also has a tool that can be used to generate the code during the deploy process.
    /// </summary>
    /// <returns>Configuration</returns>
    public RestCodeGenConfig Disable()
    {
        IsGenerationDisabled = true;
        return this;
    }
        
    /// <summary>
    /// The end point that can be called to request TypeScript for a WebApi action resources.
    /// </summary>
    /// <param name="endpoint">The relative URL.</param>
    /// <returns>The configuration.</returns>
    public RestCodeGenConfig UseEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint must be specified.", nameof(endpoint));
        }

        EndpointUrl = endpoint;
        return this;
    }
}