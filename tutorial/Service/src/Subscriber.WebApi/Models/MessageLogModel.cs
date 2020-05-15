using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Types.Attributes;

namespace Subscriber.WebApi.Models
{
    public class MessageLogModel
    {
        public string CorrelationId { get; private set; }
        public DateTime DateOccurred { get; private set; }
        public DateTime DateLogged { get; private set; }
        public string Context { get; private set; }
        public string MessageType { get; private set; }

        public object Message { get; private set; }
        public IReadOnlyDictionary<string, string> Details { get; private set; }
        public IReadOnlyDictionary<string, string> Errors { get; private set; }
        public string Hint { get; private set; }
        public bool HasErrors => Errors.Any();

        public static MessageLogModel FromEntity(MessageLog messageLog)
        {
            return new MessageLogModel
            {
                DateLogged = messageLog.DateLogged,
                Message = messageLog.Message,
                MessageType = messageLog.MessageType,
                Context = messageLog.LogContext.ToString(),
                Details = messageLog.LogDetails,
                Errors = messageLog.LogErrors,
                Hint = messageLog.Hint,

                CorrelationId = messageLog.Message.GetCorrelationId(),

                DateOccurred = messageLog.Message.Attributes.GetDateTimeValue(
                    AttributeExtensions.GetPluginScopedName("DateOccurred"),
                    messageLog.DateLogged)
            };
        }
    }
}