using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Common.Extensions.Tasks
{
    /// <summary>
    /// Extension methods for executing a collection of invokers that call an
    /// asynchronous method that will complete in the future.  When awaiting
    /// a collection of Tasks and there is an exception, only the exception 
    /// that first occurred will be thrown.  However, all of the exceptions 
    /// can be accessed since they are associated with their task.  These
    /// methods generalize the calling a collection of tasks and returning
    /// a collection of all the exceptions.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Executes a list of invokers that call an asynchronous method and returns 
        /// a collection of objects storing the invoker and its associated task.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <typeparam name="TInput">The type of value passed to all invokers.</typeparam>
        /// <param name="invokers">List of objects that will invoke an asynchronous method.</param>
        /// <param name="input">Reference to object passed to all invokers.</param>
        /// <param name="promise">The delegate called that will call the asynchronous method on invoker.</param>
        /// <returns>Collection of invokers and associated task.</returns>
        public static FutureResult<TInvoker>[] Invoke<TInvoker, TInput>(this IEnumerable<TInvoker> invokers, 
            TInput input,
            Func<TInvoker, TInput, Task> promise) 
            where TInput : class where TInvoker : class
        {
            Check.NotNull(invokers, nameof(invokers));
            Check.NotNull(input, nameof(input));
            Check.NotNull(promise, nameof(promise));

            var futureResults = new List<FutureResult<TInvoker>>();
            foreach(TInvoker invoker in invokers)
            {
                Task futureResult = promise(invoker, input);
                futureResults.Add(new FutureResult<TInvoker>(futureResult, invoker));
            }
            return futureResults.ToArray();
        }

        public static FutureResult<TInvoker>[] Invoke<TInvoker, TInput>(this IEnumerable<TInvoker> invokers,
            TInput input,
            Func<TInvoker, TInput, CancellationToken, Task> promise,
            CancellationToken cancellationToken)
            where TInput : class where TInvoker : class
        {
            Check.NotNull(invokers, nameof(invokers));
            Check.NotNull(input, nameof(input));
            Check.NotNull(promise, nameof(promise));

            var futureResults = new List<FutureResult<TInvoker>>();
            foreach (TInvoker invoker in invokers)
            {
                Task futureResult = promise(invoker, input, cancellationToken);
                futureResults.Add(new FutureResult<TInvoker>(futureResult, invoker));
            }
            return futureResults.ToArray();
        }

        /// <summary>
        /// Given a collection of future results, returns a task that will be completed
        /// when all future result tasks have completed.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <param name="futureResults">Collection of future task results.</param>
        /// <returns>Task that will complete when all future result tasks complete.</returns>
        public static Task WhenAll<TInvoker>(this FutureResult<TInvoker>[] futureResults)
            where TInvoker : class
        {
            Check.NotNull(futureResults, nameof(futureResults));
            return Task.WhenAll(futureResults.Select(fr => fr.Task));
        }

        /// <summary>
        /// Given a collection of future results returns a collection of exceptions
        /// for each future result that resulted in an exception.  The caller is 
        /// responsible for creating the exception that should be returned.
        /// </summary>
        /// <typeparam name="TInvoker">The type of object making the asynchronous call.</typeparam>
        /// <typeparam name="TEx">The type of exception returned for each failed task.</typeparam>
        /// <param name="futureResults">The list of future results to check for failed tasks.</param>
        /// <param name="exFactory">Delegate used to create exception for failed task.</param>
        /// <returns>Collection of custom exceptions for each failed task.</returns>
        public static TEx[] GetExceptions<TInvoker, TEx>(this FutureResult<TInvoker>[] futureResults,
            Func<FutureResult<TInvoker>, TEx> exFactory)
            where TInvoker : class
            where TEx : Exception
        {
            Check.NotNull(futureResults, nameof(futureResults));
            Check.NotNull(exFactory, nameof(exFactory));

            var exceptions = new List<TEx>(futureResults.Length);

            foreach (var futureResult in futureResults.Where(pt => pt.Task.Exception != null))
            {
                exceptions.Add(exFactory(futureResult));
            }
            return exceptions.ToArray();
        }
    }
}
