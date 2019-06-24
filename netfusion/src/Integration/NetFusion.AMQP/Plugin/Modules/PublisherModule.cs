using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetFusion.AMQP.Publisher;
using NetFusion.AMQP.Publisher.Internal;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types;

namespace NetFusion.AMQP.Plugin.Modules
{
    /// <summary>
    /// Module when bootstrapped finds all the IHostRegistry instances and stores for a given
    /// message type the object defined on the host (i.e. Queue/Topics) to which it should be sent.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {       
        private IEnumerable<IHostRegistry> Registries { get; set; }
        private Dictionary<Type, IHostItem> _messageHostItem;  // Message Type => Host Item
        
        public override void Initialize()
        {
            IHostItem[] hostItems = Registries
                .SelectMany(r => r.GetItems())
                .ToArray();
            
            AssertHostItems(hostItems);

            _messageHostItem = hostItems.ToDictionary(i => i.MessageType);
        }

        protected override async Task OnStopModuleAsync(IServiceProvider services)
        {
            foreach(var hostItem in _messageHostItem.Values)
            {
                var senderHostItem = (ISenderHostItem) hostItem;
                if (senderHostItem.SenderLink != null)
                {
                    await senderHostItem.SenderLink.CloseAsync();
                }
            }
        }

        public bool HasHostItem(Type messageType)
        {
            if (! messageType.IsConcreteTypeDerivedFrom<IMessage>())
            {
                throw new ArgumentException("Must of of a message type.", nameof(messageType));
            }

            return _messageHostItem.ContainsKey(messageType);
        }

        public IHostItem GetHostItem(Type messageType)
        {
            if (! HasHostItem(messageType))
            {
                throw new InvalidOperationException(
                    $"The message of type: {messageType} does not have an associated host item.");
            }

            return _messageHostItem[messageType];
        }
        
        private static void AssertHostItems(IEnumerable<IHostItem> hostItems)
        {
            var invalidMessageTypes = hostItems.WhereDuplicated(i => i.MessageType)
                .Select(di => di.FullName)
                .ToArray();

            if (invalidMessageTypes.Any())
            {
                throw new ContainerException(
                    "Message types can only be associated with one type of host item (i.e. Queue/Topic).  " + 
                    $"The following message types are invalid: {string.Join(",", invalidMessageTypes)}");    
            }
        }
    }
}