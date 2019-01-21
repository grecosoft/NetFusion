using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Azure.Messaging.Publisher;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Modules
{
    /// <summary>
    /// Module when bootstrapped finds all the INamespaceRegistry instances
    /// and stores for a given message type the object defined on the Azure
    /// namespace to which it should be sent.
    /// </summary>
    public class PublisherModule : PluginModule,
        IPublisherModule
    {
        private bool _disposed;  
        
        private IEnumerable<INamespaceRegistry> Registries { get; set; }
        private Dictionary<Type, INamespaceItem> _messageNamespace;  // Message Type => Bus Namespace
        
        public override void Initialize()
        {
            INamespaceItem[] namespaceItems = Registries
                .SelectMany(r => r.GetItems())
                .ToArray();
            
            AssertNamespaceItems(namespaceItems);

            _messageNamespace = namespaceItems.ToDictionary(i => i.MessageType);
        }

        public bool HasNamespaceItem(Type messageType)
        {
            if (! messageType.IsConcreteTypeDerivedFrom<IMessage>())
            {
                throw new ArgumentException("Must of of a message type.", nameof(messageType));
            }

            return _messageNamespace.ContainsKey(messageType);
        }

        public INamespaceItem GetNamespaceItem(Type messageType)
        {
            if (! HasNamespaceItem(messageType))
            {
                throw new InvalidOperationException(
                    $"The message of type: {messageType} does not have an associated namespace item.");
            }

            return _messageNamespace[messageType];
        }
        
        private static void AssertNamespaceItems(IEnumerable<INamespaceItem> nsItems)
        {
            var invalidMessageTypes = nsItems.WhereDuplicated(i => i.MessageType)
                .Select(di => di.FullName)
                .ToArray();

            if (invalidMessageTypes.Any())
            {
                throw new ContainerException(
                    "Message types can only be associated with one type of Namespace item (i.e. Queue/Topic).  " + 
                    $"The following message types are invalid: {string.Join(",", invalidMessageTypes)}");    
            }
        }
        
        protected override void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            foreach(var namespaceItem in _messageNamespace.Values)
            {
                var nsItem = (ILinkedItem) namespaceItem;
                nsItem.SenderLink?.Close();
            }

            _disposed = true;
        }
    }
}