using System.Threading.Tasks;

namespace NetFusion.Common.Extensions.Tasks
{
    /// <summary>
    /// Class associating a Task with the object instance responsible 
    /// for starting the asynchronous call pending completion. 
    /// </summary>
    /// <typeparam name="TInvoker">The type of the invoker.</typeparam>
    public class FutureResult<TInvoker>
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
        public FutureResult(Task task, TInvoker invoker)
        {
            Check.NotNull(task, nameof(task));
            Check.NotNull(invoker, nameof(invoker));

            this.Task = task;
            this.Invoker = invoker;
        }
    }
}
