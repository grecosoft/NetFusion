using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.RabbitMQ.Settings;

namespace NetFusion.RabbitMQ
{
   /// <summary>
   /// Adds the types specific to the RabbitMQ plug-in.  The extension
   /// methods contained within this class are used only for unit-testing.
   /// The mock plug-in classes contained within NetFusion.Test implement
   /// IPluginTypeAccessor.
   /// </summary>
   public static class PluginTypeAccessorExtensions
   {
       /// <summary>
       /// Adds the needed plug-in types specific to the plug-in.
       /// </summary>
       /// <param name="plugin">Mock plug-in instance.</param>
       /// <returns>Reference to the plug-in for method chaining.</returns>
       public static IPluginTypeAccessor UseRabbitMqPlugin(this IPluginTypeAccessor plugin)
       {
           plugin
               .AddPluginType<BusModule>()
               .AddPluginType<PublisherModule>()
               .AddPluginType<SubscriberModule>()
               .AddPluginType<BusSettings>();

           return plugin;
       }
   }
}