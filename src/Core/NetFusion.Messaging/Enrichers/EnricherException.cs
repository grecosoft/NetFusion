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
        /// <param name="taskList">The future result containing the exception.</param>
        public EnricherException(TaskListItem<IMessageEnricher> taskList)
        {
            if (taskList == null) throw new NullReferenceException(nameof(taskList));

            var taskException = taskList.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Enricher", taskList.Invoker.GetType().FullName }
            };
        }
    }
}
