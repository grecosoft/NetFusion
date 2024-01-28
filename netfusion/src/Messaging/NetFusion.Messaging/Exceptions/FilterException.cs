using NetFusion.Common.Base.Exceptions;
using NetFusion.Messaging.Filters;

namespace NetFusion.Messaging.Exceptions;

/// <summary>
/// Contains information about a query filter that threw an exception
/// when applied.
/// </summary>
public class FilterException : NetFusionException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="filter">The associated filter that raised exception.</param>
    /// <param name="aggregateException">The aggregate exception associated with task.</param>
    public FilterException(string message, IMessageFilter filter, AggregateException? aggregateException) 
        : base(message, aggregateException)
    {
        ArgumentNullException.ThrowIfNull(filter);

        Details["Filter"] = filter.GetType().FullName!;
    }
}