using System;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// An object representing a query dispatched and handled by a consumer.
    /// </summary>
    public abstract class Query : IQuery
    {
        /// <summary>
        /// The result of the query provided by the consumer.
        /// </summary>
        public object Result { get; protected set; }

        internal Query()
        {

        }

        // The type of the result declared by the query.  Base abstract
        // class does not have a declared result type.
        public Type DeclaredResultType { get; protected set; }

        /// <summary>
        /// Called with the result returned from the consumer.
        /// </summary>
        /// <param name="result">The consumer's result.</param>
        public virtual void SetResult(object result)
        {
            // The query result can be null.
            if (result == null) return;
            
            if (! result.GetType().CanAssignTo(DeclaredResultType))
            {
                throw new InvalidOperationException(
                    $"The handler for the query of type: {GetType()} returned a result of type: {result.GetType()} " + 
                    $"and is not assignable to the query's declared result type of: {DeclaredResultType}.");
            }

            Result = result;
        }
    }

    /// <summary>
    /// An object representing a query, with a declared result type, dispatched 
    /// and handled by a consumer.
    /// </summary>
    /// <typeparam name="TResult">The type of the result expected by the query.</typeparam>
    public abstract class Query<TResult> : Query, IQuery<TResult>
    {
        protected Query()
        {
            DeclaredResultType = typeof(TResult);
            base.Result = default(TResult);
        }

        // The type of the result declared by the query.
        Type IQuery.DeclaredResultType => typeof(TResult);

        /// <summary>
        /// The result of the query provided by the consumer.
        /// </summary>
        public new TResult Result => (TResult)base.Result;
    }
}
