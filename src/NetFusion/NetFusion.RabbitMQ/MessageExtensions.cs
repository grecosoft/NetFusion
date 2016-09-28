using NetFusion.Common;
using NetFusion.Messaging;
using System;
using System.Linq;

namespace NetFusion.RabbitMQ
{
    /// <summary>
    /// Extension methods used to add dynamic attribute values to a message 
    /// that are used during publishing and consuming the message.  These
    /// are basically header values and the message properties are the body
    /// of the message.
    /// </summary>
    public static class MessageExtensions
    {
        private static readonly Type Context = typeof(RabbitMQManifest);

        public static void SetAcknowledged(this IMessage message, bool value = true)
        {
            message.Attributes.SetValue(value, Context);
        }

        public static bool GetAcknowledged(this IMessage message)
        {
            return message.Attributes.GetValue(false, Context);
        }

        public static void SetRejected(this IMessage message, bool value = true, bool requeue = true)
        {
            message.Attributes.SetValue(value, Context);
            message.SetRequeueOnRejection(requeue);
        }

        public static bool GetRejected(this IMessage message)
        {
            return message.Attributes.GetValue(false, Context);
        }

        private static void SetRequeueOnRejection(this IMessage message, bool requeue)
        {
            message.Attributes.SetValue(requeue, Context);
        }

        public static bool GetRequeueOnRejection(this IMessage message)
        {
            return message.Attributes.GetValue(true, Context);
        }

        public static void SetRouteKey(this IMessage message, string value)
        {
            Check.NotNullOrWhiteSpace(value, nameof(value));

            message.Attributes.SetValue(value.ToUpper(), Context);
        }

        public static void SetRouteKey(this IMessage message, params object[] args)
        {
            string[] argValues = args.Select(a => a?.ToString()?.ToUpper()).ToArray();
            message.Attributes.SetValue(String.Join(".", argValues), Context);
        }

        public static string GetRouteKey(this IMessage message)
        {
            return message.Attributes.GetValue<string>(null, Context);
        }

        public static void SetContentType(this IMessage message, string value)
        {
            message.Attributes.SetValue(value, Context);
        }

        public static string GetContentType(this IMessage message)
        {
            return message.Attributes.GetValue<string>(null, Context);
        }

        public static void SetCorrelationId(this IMessage message, string value)
        {
            message.Attributes.SetValue(value, Context);
        }

        public static string GetCorrelationId(this IMessage message)
        {
            return message.Attributes.GetValue<string>(null, Context);
        }
    }
}
