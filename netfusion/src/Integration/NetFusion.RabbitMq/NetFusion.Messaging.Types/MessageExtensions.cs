using System;
using System.Linq;

namespace NetFusion.Messaging.Types
{
    public static class MessageExtensions2
    {
        private static readonly Type Context = typeof(MessageExtensions);

        public static void SetRouteKey(this IMessage message, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value can't be null or empty string.", nameof(value));
            
            message.Attributes.SetMemberValue(value, Context);
        }

        public static void SetRouteKey(this IMessage message, params object[] values)
        {
            string[] routeKeys = values.Where(v => v != null)
                .Select(v => v.ToString()).ToArray();

            message.Attributes.SetMemberValue(string.Join(".", routeKeys), Context);
        }

        public static string GetRouteKey(this IMessage message)
        {
            return message.Attributes.GetMemberValueOrDefault<string>(null, Context);
        }

        public static byte? GetPriority(this IMessage message) =>
            message.Attributes.GetValueOrDefault<byte?>("Priority", null, Context);
    }
}