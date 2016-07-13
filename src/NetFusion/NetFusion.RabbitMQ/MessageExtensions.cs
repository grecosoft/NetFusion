using System;
using System.Text;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace NetFusion.RabbitMQ
{
    public static class MessageExtensions
    {
        private static readonly Type Context = typeof(RabbitMQManifest);

        public static void SetAcknowledged(this IMessage message, bool value = true)
        {
            message.SetObjectAttribute(value, Context);
        }

        public static bool GetAcknowledged(this IMessage message)
        {
            return message.GetObjectAttribute(false, Context);
        }

        public static void SetRejected(this IMessage message, bool value = true, bool requeue = true)
        {
            message.SetObjectAttribute(value, Context);
            message.SetRequeueOnRejection(requeue);
        }

        public static bool GetRejected(this IMessage message)
        {
            return message.GetObjectAttribute(false, Context);
        }

        private static void SetRequeueOnRejection(this IMessage message, bool requeue)
        {
            message.SetObjectAttribute(requeue, Context);
        }

        public static bool GetRequeueOnRejection(this IMessage message)
        {
            return message.GetObjectAttribute(true, Context);
        }

        public static void SetRouteKey(this IMessage message, string value)
        {
            message.SetObjectAttribute(value, Context);
        }

        public static void SetRouteKey(this IMessage message, params object[] args)
        {
            message.SetObjectAttribute(String.Join(".", args), Context);
        }

        public static string GetRouteKey(this IMessage message)
        {
            return message.GetObjectAttribute<string>(null, Context);
        }

        public static void SetInvokeLocalConsumers(this IMessage message, bool value = true)
        {
            message.SetObjectAttribute(value, Context);
        }

        public static bool GetInvokeLocalConsumers(this IMessage message)
        {
            return message.GetObjectAttribute(true, Context);
        }

        public static void SetCorrelationId(this IMessage message, string value)
        {
            message.SetObjectAttribute(value, Context);
        }

        public static string GetCorrelationId(this IMessage message)
        {
            return message.GetObjectAttribute<string>(null, Context);
        }
    }
}
