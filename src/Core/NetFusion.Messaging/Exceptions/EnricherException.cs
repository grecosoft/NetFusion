using System;
using System.Collections.Generic;
using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Enrichers;

namespace NetFusion.Messaging.Exceptions
{
    /// <summary>
    /// Contains information about a message enricher that throws and exception when applied.
    /// </summary>
    public class EnricherException : NetFusionException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="taskItem">The task item containing the exception.</param>
        public EnricherException(TaskListItem<IMessageEnricher> taskItem)
            :base("Enricher Exception", GetSourceException(taskItem))
        {
            if (taskItem == null) throw new NullReferenceException(nameof(taskItem));

            var sourceException = GetSourceException(taskItem);

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Enricher", taskItem.Invoker.GetType().FullName }
            };
        }

        private static Exception GetSourceException(TaskListItem<IMessageEnricher> taskItem)
        {
            var taskException = taskItem.Task.Exception;
            return taskException.InnerException;
        }
    }
}
