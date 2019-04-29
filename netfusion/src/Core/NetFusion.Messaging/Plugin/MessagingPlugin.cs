using NetFusion.Bootstrap.Refactors;
using NetFusion.Messaging.Config;

namespace NetFusion.Messaging.Plugin
{
    public class MessagingPlugin : PluginDefinition
    {
        public override string PluginId => "4576D809-E216-4C03-BE43-737728047BAA";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.CorePlugin;
        public override string Name => "Messaging Plug-in"; 

        public MessagingPlugin()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Core/NetFusion.Messaging";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/core.messaging.overview";
            
            Description =  "Contains common implementation for handling Commands, Domain-Events and Queries in-process that " +
                           "can be extended by other plug-ins to publish Commands and Domain-Event messages out of process."; 
            
            // Configurations:
            AddConfig<MessageDispatchConfig>();
            AddConfig<QueryDispatchConfig>();
            
            // Modules:
            AddModule<MessagingModule>();
            AddModule<MessageDispatchModule>();
            AddModule<MessageEnricherModule>();
         
            AddModule<QueryDispatchModule>();
            AddModule<QueryFilterModule>();
        }   
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddMessaging(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<MessagingPlugin>();
            return composite;
        }
    }
}