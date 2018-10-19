using Amqp;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Publisher
{
    /// <summary>
    /// Metadata for a queue defined on a namespace.
    /// </summary>
    /// <typeparam name="TCommand">The type of command assocated with queue.</typeparam>
    public class Queue<TCommand> : NamespaceItem<TCommand>
        where TCommand : ICommand
    {
        public Queue(string namespaceName, string name) 
            : base( namespaceName, name)
        {
            
        }

        internal override void SetMessageProperties(IMessage message, Message nsMessage)
        {
            base.SetMessageProperties(message, nsMessage);
        }
    }
}