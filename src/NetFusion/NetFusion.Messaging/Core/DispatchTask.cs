using NetFusion.Common;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// The task used to call a corresponding message dispatcher.
    /// </summary>
    public class DispatchTask
    {
        public Task Task { get; }
        public MessageDispatchInfo Dispatch { get; }

        public DispatchTask(Task task, MessageDispatchInfo dispatch)
        {
            Check.NotNull(task, nameof(task));
            Check.NotNull(dispatch, nameof(dispatch));

            this.Task = task;
            this.Dispatch = dispatch;
        }
    }
}
