using NetFusion.AMQP.Publisher.Internal;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.AMQP.Publisher
{
    /// <summary>
    /// Metadata for a queue defined on a host.
    /// </summary>
    /// <typeparam name="TCommand">The type of command associated with queue.</typeparam>
    public class Queue<TCommand> : HostItem<TCommand>
        where TCommand : ICommand
    {
        public Queue(string hostName, string name) 
            : base( hostName, name)
        {
            
        }
    }
}