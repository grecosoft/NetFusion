using NetFusion.Base.Exceptions;
using NetFusion.Common;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Enrichers;
using System.Collections.Generic;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Contains information about a message enricher that throws and exception when applied.
    /// </summary>
    public class EnricherException : NetFusionException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="futureResult">The future result containing the exception.</param>
        public EnricherException(TaskListItem<IMessageEnricher> futureResult)
        {
             Check.NotNull(futureResult, nameof(futureResult));

            var taskException = futureResult.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Enricher", futureResult.Invoker.GetType().FullName }
            };
        }
    }
}
