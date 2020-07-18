using System;
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
                return;
            }

            _resourceTypes = Context.AllAppPluginTypes
                .Where(t => t.HasAttribute<ExposedNameAttribute>())
                .ToArray();
        }

        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IApiCodeGenService, ApiCodeGenService>();
        }

        // Generates the Typescript for the models exposed by the Microservice's
        // REST API.  These generated files then can be served back to the calling
        // client when viewing a given API's documentation.
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            try
            {
                Context.Logger.LogInformation("Generating TypeScript for Microservice REST API.");

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
    }
}
