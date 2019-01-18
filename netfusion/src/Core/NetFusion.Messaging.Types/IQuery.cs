using NetFusion.Base.Plugins;
using System;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Represents a query request that can be dispatched to a consumer.
    /// </summary>
    public interface IQuery : IKnownPluginType
    {
        /// <summary>
        /// The result provided by the consumer.
        /// </summary>
        object Result { get; }

        /// <summary>
        /// The type of the request expected by the query from the consumer.
        /// This can be used by a query filter to automatically map the actual
        /// return type into the expected consumer result type.
        /// </summary>
        Type DeclaredResultType { get; }

        /// <summary>
        /// Sets the result to be associated with the query.
        /// </summary>
        /// <param name="result">The result of the query.</param>
        void SetResult(object result);
    }

    /// <summary>
    /// Represents a query request that can be dispatched to a consumer.
    /// </summary>
    /// <typeparam name="TResult">The type of the result expected by the query.</typeparam>
    public interface IQuery<out TResult> : IQuery
    {
        /// <summary>
        /// The result of the query's execution.
        /// </summary>
        new TResult Result { get; }
    }
}
