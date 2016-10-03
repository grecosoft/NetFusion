using NetFusion.Common;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// The task used to call a corresponding message dispatcher.
    /// </summary> 
    public class DispatchTask
    {
        /// <summary>
        /// The task dispatching the message.
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// The associated dispatch information used to dispatch the message
        /// to its corresponding consumer handler.
        /// </summary>
        public MessageDispatchInfo Dispatch { get; }

        /// <summary>
        /// Associates a task with its corresponding dispatcher.
        /// </summary>
        /// <param name="task">The task dispatching the message.</param>
        /// <param name="dispatch">The dispatch information used to dispatch message.</param>
        public DispatchTask(Task task, MessageDispatchInfo dispatch)
        {
            Check.NotNull(task, nameof(task));
            Check.NotNull(dispatch, nameof(dispatch));

            this.Task = task;
            this.Dispatch = dispatch;
        }
    }
}
