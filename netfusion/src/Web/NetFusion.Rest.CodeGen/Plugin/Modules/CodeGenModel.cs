using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.CodeGen.Core;
using NetFusion.Rest.CodeGen.Plugin.Configs;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using TypeGen.Core.Generator;

namespace NetFusion.Rest.CodeGen.Plugin.Modules
{
    /// <summary>
    /// Plugin module called during the bootstrap process.  This module generates TypeScript
    /// classes for all WebApi models marked with ExposedNameAttribute.  These models represent
    /// the data returned and sent to WebApi methods used by web-based clients.
    /// </summary>
    public class CodeGenModule : PluginModule,
        ICodeGenModule
    {
        public RestCodeGenConfig CodeGenConfig { get; private set; }
        private Type[] _resourceTypes;

        public override void Initialize()
        {
            CodeGenConfig = Context.Plugin.GetConfig<RestCodeGenConfig>();
            if (CodeGenConfig.IsGenerationDisabled)
            {
                _resourceTypes = new Type[] { };
                return;
            }

            // Any application centric types marked with the ExposedName Attribute
            // are considered part of the Microservice's public API.
            _resourceTypes = Context.AllAppPluginTypes
                .Where(t => t.HasAttribute<ResourceAttribute>())
                .ToArray();
        }

        public IReadOnlyCollection<Type> ResourceTypes => _resourceTypes;

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IApiCodeGenService, ApiCodeGenService>();
        }

        // Generates the Typescript for the models exposed by the Microservice's
        // REST API.  These generated files then can be served back to the calling
        // client when viewing a given API's documentation.
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            if (CodeGenConfig.IsGenerationDisabled)
            {
                Context.Logger.LogWarning("TypeScript code generation is disabled.");
                return base.OnRunModuleAsync(services);
            }
            
            Context.Logger.LogInformation("Generating TypeScript for Microservice REST API.");
            
            try
            {
                DeleteExistingGeneratedFiles();
                
                var codeGenerator = new Generator(new GeneratorOptions {
                    BaseOutputDirectory = CodeGenConfig.CodeGenerationDirectory });

                var codeSpec = new ResourceGenerationSpec(_resourceTypes);
                var files = codeGenerator.Generate(new[] { codeSpec });

                Context.Logger.LogInformation(
                    $"TypeScript Code Generation Completed for {files.Count()} files.  See Composite Log for Details.");
            }
            catch (Exception ex)
            {
                Context.Logger.LogError(ex, "There was an issue generating TypeScript.");
            }

            return base.OnStartModuleAsync(services);
        }

        private void DeleteExistingGeneratedFiles()
        {
            if (Directory.Exists(CodeGenConfig.CodeGenerationDirectory))
            {
                Directory.Delete(CodeGenConfig.CodeGenerationDirectory, true);
            }
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Disabled"] = CodeGenConfig.IsGenerationDisabled;
            moduleLog["OutputDirectory"] = CodeGenConfig.CodeGenerationDirectory;
            moduleLog["Resources"] = _resourceTypes.Select(rt => new
            {
                ResourceName = rt.GetResourceName(),
                ImplementationType = rt.FullName
            }).ToArray();
        }
    }
}
