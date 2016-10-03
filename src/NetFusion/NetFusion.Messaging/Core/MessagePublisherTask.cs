using NetFusion.Common;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// The task used to call a corresponding message publisher.
    /// </summary> 
    public class MessagePublisherTask
    {
        /// <summary>
        /// The future result returned from the publisher.
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// The called publisher.
        /// </summary>
        public IMessagePublisher Publisher { get; }

        /// <summary>
        /// Associates a task with the invoked publisher.
        /// </summary>
        /// <param name="task">The future result returned from the publisher.</param>
        /// <param name="publisher">The called publisher from which a response is awaiting.</param>
        public MessagePublisherTask(Task task, IMessagePublisher publisher)
        {
            Check.NotNull(task, nameof(task));
            Check.NotNull(publisher, nameof(publisher));

            this.Task = task;
            this.Publisher = publisher;
        }
    }
}
