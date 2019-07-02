using System;
using System.Reflection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Internal;

namespace NetFusion.Redis.Subscriber.Internal
{
    /// <summary>
    /// Class associating a Redis channel subscription which the
    /// dispatch info of the handling method.
    /// </summary>
    public class MessageChannelSubscriber
    {
        public MessageDispatchInfo DispatchInfo { get; }
        
        public string DatabaseName { get; }
        public string Channel { get; }
  
        public MessageChannelSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));
            
            // Obtain the subscriber attribute so the metadata
            // can be retrieved.
            var channelAttrib = dispatchInfo.MessageHandlerMethod
                .GetCustomAttribute<ChannelSubscriptionAttribute>();

            DispatchInfo = dispatchInfo;
            DatabaseName = channelAttrib.DatabaseName;
            Channel = channelAttrib.Channel;
        }
        
        // Any dispatch corresponding to a method decorated with a
        // ChannelSubscriptionAttribute attribute will be bound to a channel.
        public static bool IsSubscriber(MessageDispatchInfo dispatchInfo)
        {
            if (dispatchInfo == null)
                throw new ArgumentNullException(nameof(dispatchInfo));

            return dispatchInfo.MessageHandlerMethod.HasAttribute<ChannelSubscriptionAttribute>();
        }   
    }
}