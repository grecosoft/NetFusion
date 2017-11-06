using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Common.Extensions.Tasks
{
    /// <summary>
    /// Extension methods for executing a collection of invokers that call an asynchronous method that will  complete 
    /// in the future.  When awaiting a collection of Tasks and there is an exception, only the exception  that first 
    /// occurred will be thrown.  However, all of the exceptions can be accessed since they are associated with their 
    /// task.  These methods generalize the calling a collection of tasks and returning a collection of all the exceptions.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Executes a list of invokers that call an asynchronous method and returns a collection of objects 
        /// storing  the invoker and its associated task.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <typeparam name="TInput">The type of value passed to all invokers.</typeparam>
        /// <param name="invokers">List of objects that will invoke an asynchronous method.</param>
        /// <param name="input">Reference to object passed to all invokers.</param>
        /// <param name="promise">The delegate invoked that will call the asynchronous method on passed invoker.</param>
        /// <returns>Collection of invokers and associated task.</returns>
        public static TaskListItem<TInvoker>[] Invoke<TInvoker, TInput>(this IEnumerable<TInvoker> invokers, 
            TInput input,
            Func<TInvoker, TInput, Task> promise) 
            where TInput : class where TInvoker : class
        {
            if (invokers == null) throw new ArgumentNullException(nameof(invokers));
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (promise == null) throw new ArgumentNullException(nameof(promise));    

            var taskList = new List<TaskListItem<TInvoker>>();
            foreach(TInvoker invoker in invokers)
            {
                Task futureResult = promise(invoker, input);
                taskList.Add(new TaskListItem<TInvoker>(futureResult, invoker));
            }
            return taskList.ToArray();
        }

        /// <summary>
        /// Executes a list of invokers that call an asynchronous method and returns a collection of objects 
        /// storing  the invoker and its associated task.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <typeparam name="TInput">The type of value passed to all invokers.</typeparam>
        /// <param name="invokers">List of objects that will invoke an asynchronous method.</param>
        /// <param name="input">Reference to object passed to all invokers.</param>
        /// <param name="promise">The delegate invoked that will call the asynchronous method on passed invoker.</param>
        /// <param name="cancellationToken">Cancellation token used to signal if the asynchronous task should be canceled.</param>
        /// <returns>Collection of invokers and associated task.</returns>
        public static TaskListItem<TInvoker>[] Invoke<TInvoker, TInput>(this IEnumerable<TInvoker> invokers,
            TInput input,
            Func<TInvoker, TInput, CancellationToken, Task> promise,
            CancellationToken cancellationToken)
            where TInput : class where TInvoker : class
        {
            if (invokers == null) throw new ArgumentNullException(nameof(invokers));
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (promise == null) throw new ArgumentNullException(nameof(promise));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            var taskList = new List<TaskListItem<TInvoker>>();
            foreach (TInvoker invoker in invokers)
            {
                Task futureResult = promise(invoker, input, cancellationToken);
                taskList.Add(new TaskListItem<TInvoker>(futureResult, invoker));
            }
            return taskList.ToArray();
        }

        /// <summary>
        /// Given a task list, returns a task that will be completed when all task list items have completed.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <param name="taskList">Collection of task item results.</param>
        /// <returns>Task that will completed when all task list items complete.</returns>
        public static Task WhenAll<TInvoker>(this TaskListItem<TInvoker>[] taskList)
            where TInvoker : class
        {
            if (taskList == null) throw new ArgumentNullException(nameof(taskList));
            return Task.WhenAll(taskList.Select(fr => fr.Task));
        }

        /// <summary>
        /// Given a task list, returns a collection of exceptions for each task item that resulted 
        /// in an exception.  The caller is  responsible for creating the exception that should be 
        /// returned.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <typeparam name="TEx">The type of exception returned for each failed task.</typeparam>
        /// <param name="taskList">The task list to check for failed tasks.</param>
        /// <param name="exFactory">Delegate used to create exception for failed task.</param>
        /// <returns>Collection of custom exceptions for each failed task.</returns>
        public static TEx[] GetExceptions<TInvoker, TEx>(this TaskListItem<TInvoker>[] taskList,
            Func<TaskListItem<TInvoker>, TEx> exFactory)
            where TInvoker : class
            where TEx : Exception
        {
            if (taskList == null) throw new ArgumentNullException(nameof(taskList));
            if (exFactory == null) throw new ArgumentNullException(nameof(exFactory));
            
            var exceptions = new List<TEx>(taskList.Length);

            foreach (var futureResult in taskList.Where(pt => pt.Task.Exception != null))
            {
                exceptions.Add(exFactory(futureResult));
            }
            return exceptions.ToArray();
        }
    }
}
