using System;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Extension methods used to add dynamic attribute values to a message 
    /// that are used during publishing and consuming the message.  These
    /// are basically header values and the message properties are the body
    /// of the message.
    /// </summary>
    public static class MessageExtensions
    {
        private static readonly Type Context = typeof(MessageExtensions);

        public static void SetCorrelationId(this IMessage message, string value)
        {
            message.Attributes.SetMemberValue(value, Context, overrideIfPresent: false);
        }

        public static string GetCorrelationId(this IMessage message)
        {
            return message.Attributes.GetMemberValueOrDefault<string>(null, Context);
        }
    }
}
