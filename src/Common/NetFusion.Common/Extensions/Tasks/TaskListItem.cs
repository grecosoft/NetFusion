using System;
using System.Threading.Tasks;

namespace NetFusion.Common.Extensions.Tasks
{
    /// <summary>
    /// Class associating a Task with the object instance responsible 
    /// for starting the asynchronous call pending completion. 
    /// </summary>
    /// <typeparam name="TInvoker">The type of the invoker.</typeparam>
    public class TaskListItem<TInvoker>
        where TInvoker : class
    {
        /// <summary>
        /// The task associated with the work being completed.
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// The object instance that started the asynchronous call.
        /// </summary>
        public TInvoker Invoker { get; }

        /// <summary>
        /// Associates task with invoker.
        /// </summary>
        /// <param name="task">The task associated with the work being completed.</param>
        /// <param name="invoker">The object instance that started the asynchronous call.</param>
        public TaskListItem(Task task, TInvoker invoker)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));
            
            Task = task;
            Invoker = invoker;
        }
    }
}
