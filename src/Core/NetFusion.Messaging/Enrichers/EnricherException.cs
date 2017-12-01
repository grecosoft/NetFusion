using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions.Tasks;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Contains information about a message enricher that throws and exception when applied.
    /// </summary>
    public class EnricherException : NetFusionException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="taskItem">The future result containing the exception.</param>
        public EnricherException(TaskListItem<IMessageEnricher> taskItem)
        {
            if (taskItem == null) throw new NullReferenceException(nameof(taskItem));

            var taskException = taskItem.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Enricher", taskItem.Invoker.GetType().FullName }
            };
        }
    }
}
