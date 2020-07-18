using System;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.CodeGen.Plugin.Configs;
using NetFusion.Rest.CodeGen.Plugin.Modules;

namespace NetFusion.Rest.CodeGen.Plugin
{
    public class CodeGenPlugin : PluginBase
    {
        public override string PluginId => "6026738F-5EA5-4978-9203-EC25297632A9";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: REST Code-Generation Plugin";

        public CodeGenPlugin()
        {
     
            AddConfig<RestCodeGenConfig>();
            AddModule<CodeGenModule>();

            Description = "Plugin implementing management and querying of REST/HAL API documentation";
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Web/NetFusion.Rest.CodeGen";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki";
        }
    }
        
    public static class CompositeBuilderExtensions
    {
        /// <summary>
        /// Adds the REST Code-Generation Plugin to the composite application container.
        /// </summary>
        /// <param name="composite">The composite container builder.</param>
        /// <returns>Reference to the composite container builder.</returns>
        public static ICompositeContainerBuilder AddRestCodeGen(this ICompositeContainerBuilder composite)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            
            // Add plugin for Rest API code generation support:
            composite.AddPlugin<CodeGenPlugin>();
            
            return composite;
        }
    }
}