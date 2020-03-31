using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    /// Contains information used to dispatch a query to its corresponding consumer.
    /// </summary>
    public class QueryDispatchInfo
    {
        /// <summary>
        /// The type of the query.
        /// </summary>
        public Type QueryType { get; }

        /// <summary>
        /// The type of the consumer to be invoked to execute the query.
        /// </summary>
        public Type ConsumerType { get; }
       
        /// <summary>
        /// Indicates the consumer's query handler method is asynchronous.
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// Indicates that the consumer's query handler returns an asynchronous result.
        /// </summary>
        public bool IsAsyncWithResult { get; }

        /// <summary>
        /// Indicates that the consumer's query handler has a defined cancellation token parameter.
        /// </summary>
        public bool IsCancellable { get; }

        /// <summary>
        /// The consumer method to call for query execution.
        /// </summary>
        public MethodInfo HandlerMethod { get;  }

        /// <summary>
        /// Delegate to invoke the message handler method on the consumer at runtime.
        /// </summary>
        public MulticastDelegate Invoker { get; }

      
        public QueryDispatchInfo(MethodInfo methodInfo)
        {
            HandlerMethod = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            QueryType = GetQueryParamType(methodInfo);
            ConsumerType = methodInfo.DeclaringType;

            IsAsync = methodInfo.IsAsyncMethod();
            IsAsyncWithResult = methodInfo.IsAsyncMethodWithResult();
            IsCancellable = methodInfo.IsCancellableMethod();
    
            Invoker = CreateHandlerDelegate();
        }

        private static Type GetQueryParamType(MethodInfo queryHandler)
        {
            return queryHandler.GetParameterTypes()
                .First(pt => pt.CanAssignTo<IQuery>());
        }

        private MulticastDelegate CreateHandlerDelegate()
        {
            // Required Handler Parameters:
            var paramTypes = new List<Type>
            {
                ConsumerType,                        
                QueryType                  
            };

            // Optional Handler Parameters:
            if (IsCancellable)
            {
                paramTypes.Add(typeof(CancellationToken));
            }

            paramTypes.Add(HandlerMethod.ReturnType);

            var dispatchType = Expression.GetDelegateType(paramTypes.ToArray());
            return (MulticastDelegate)HandlerMethod.CreateDelegate(dispatchType);
        }

        // Dispatches the query to the handling consumer.
        public async Task<object> Dispatch(IQuery query, IQueryConsumer consumer, CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (consumer == null) throw new ArgumentNullException(nameof(consumer));

            var taskSource = new TaskCompletionSource<object>();

            try
            {
                if (IsAsync)
                {
                    var invokeParams = new List<object> { consumer, query };
                    if (IsCancellable)
                    {
                        invokeParams.Add(cancellationToken);
                    }

                    var asyncResult = (Task)Invoker.DynamicInvoke(invokeParams.ToArray());
                    await asyncResult.ConfigureAwait(false);

                    object result = ProcessResult(query, asyncResult);
                    taskSource.SetResult(result);
                }
                else
                {
                    object syncResult = Invoker.DynamicInvoke(consumer, query);
                    object result = ProcessResult(query, syncResult);
                    taskSource.SetResult(result);
                }
            }
            catch (Exception ex)
            {
                var invokeEx = ex as TargetInvocationException;
                var sourceEx = ex;

                if (invokeEx != null)
                {
                    sourceEx = invokeEx.InnerException;
                }

                var dispatchEx = new QueryDispatchException("Exception Dispatching Query to Consumer.",
                    this,
                    sourceEx);

                taskSource.SetException(dispatchEx);
            }

            return await taskSource.Task.ConfigureAwait(false);
        }

        private object ProcessResult(IQuery query, object result)
        {
            // The query request is a Task so get the result from the
            // task and set it as the query's result:
            if (result != null && IsAsyncWithResult)
            {
                dynamic resultTask = result;
                var resultValue = (object)resultTask.Result;
                
                query.SetResult(resultValue);
                return resultValue;
            }

            // If the handler was not asynchronous set the result of the query:
            if (result != null && !IsAsync)
            {
                query.SetResult(result);
                return result;
            }

            return null;
        }
    }
}
