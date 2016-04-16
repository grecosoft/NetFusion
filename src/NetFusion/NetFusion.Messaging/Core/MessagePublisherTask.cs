using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// The task used to call a corresponding message publisher.
    /// </summary>
    public class MessagePublisherTask
    {
        public Task Task { get; }
        public IMessagePublisher Publisher { get; }

        public MessagePublisherTask(Task task, IMessagePublisher eventDispatcher)
        {
            this.Task = task;
            this.Publisher = eventDispatcher;
        }
    }
}
