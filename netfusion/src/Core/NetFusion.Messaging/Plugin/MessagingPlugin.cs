using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Plugin.Modules;

namespace NetFusion.Messaging.Plugin
{
    public class MessagingPlugin : PluginBase
    {
        public override string PluginId => "4576D809-E216-4C03-BE43-737728047BAA";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: Messaging"; 

        public MessagingPlugin()
        {
            AddConfig<MessageDispatchConfig>();
            AddModule<MessagingModule>();
            AddModule<MessageDispatchModule>();
            AddModule<MessageEnricherModule>();
         
            AddConfig<QueryDispatchConfig>();
            AddModule<QueryDispatchModule>();
            AddModule<QueryFilterModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Core/NetFusion.Messaging";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/core.messaging.overview";
            
            Description =  "Contains common implementation for handling Commands, Domain-Events and Queries in-process that " +
                           "can be extended by other plug-ins to publish Commands and Domain-Event messages out of process."; 
        }   
    }
    
    public static class CompositeBuilderExtensions
    {
        /// <summary>
        /// Add messaging plugin used for in-process messaging and that can be extended
        /// by other plugins by registering message-publishers.
        /// </summary>
        /// <param name="composite">Reference to the composite container builder.</param>
        /// <returns>Reference to the composite container builder.</returns>
        public static ICompositeContainerBuilder AddMessaging(this ICompositeContainerBuilder composite)
        {
            return composite.AddPlugin<MessagingPlugin>();
        }
    }
}