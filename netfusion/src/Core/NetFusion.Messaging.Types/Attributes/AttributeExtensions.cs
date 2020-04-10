using System;
using System.Linq;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types.Attributes
{
    public static class AttributeExtensions
    {
        /// <summary>
        /// Returns the supplied name prefixed with a namespace specific
        /// for any attributes set by this plugin.
        /// </summary>
        /// <param name="name">The base name to prefix.</param>
        /// <returns>The complete name.</returns>
        public static string GetPluginScopedName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name not specified", nameof(name));
            
            return $"net-fusion.messaging.{name}";
        }

        /// <summary>
        /// Set a value used to identify the message if not already set.
        /// </summary>
        /// <param name="message">The message to set correlation value.</param>
        /// <param name="value">The value to identify the message.</param>
        public static void SetCorrelationId(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetPluginScopedName("CorrelationId"), value, false);
      
        /// <summary>
        /// Reads the value used to identify the message.  If not present, null is returned.
        /// </summary>
        /// <param name="message">The message to read correlation value.</param>
        /// <returns>The value used to identity the message.</returns>
        public static string GetCorrelationId(this IMessage message) =>
            message.Attributes.GetStringValue(GetPluginScopedName("CorrelationId"), null);
       
        /// <summary>
        /// An arbitrary string value associated with the message.
        /// </summary>
        /// <param name="message">The message to set route key.  If present, it
        /// will be overriden.</param>
        /// <param name="value">The arbitrary string value.</param>
        public static void SetRouteKey(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetPluginScopedName("RouteKey"), value);
        

        /// <summary>
        /// Given an array of values, set the route key to the corresponding string
        /// values concatenated with period character.
        /// </summary>
        /// <param name="message">The message to set route key.  If present, it
        /// will be overriden.</param>
        /// <param name="values">Array of values to be encoded in the route key.</param>
        public static void SetRouteKey(this IMessage message, params object[] values)
        {
            string[] routeKeys = values.Where(v => v != null)
                .Select(v => v.ToString()).ToArray();

            message.Attributes.SetStringValue(GetPluginScopedName("RouteKey"), routeKeys);
        }

        /// <summary>
        /// Returns the route key specified for the message.
        /// </summary>
        /// <param name="message">The message to return associated route key.</param>
        /// <returns>The assigned route key value.  If not present, null is returned.</returns>
        public static string GetRouteKey(this IMessage message)
        {
            return message.Attributes.GetStringValue(GetPluginScopedName("RouteKey"), null);
        }

        /// <summary>
        /// Set the priority value associated with the message in relation to other messages.
        /// </summary>
        /// <param name="message">The message to set priority.</param>
        /// <param name="value">The priority value.</param>
        public static void SetPriority(this IMessage message, byte value) =>
            message.Attributes.SetByteValue(GetPluginScopedName("Priority"), value);

        /// <summary>
        /// Returns a priority value associated with the message in relation to other messages.
        /// </summary>
        /// <param name="message">The message to return associated priority.</param>
        /// <returns>The message's priority.</returns>
        public static byte? GetPriority(this IMessage message)
        {
            if (message.Attributes.HasValue(GetPluginScopedName("Priority")))
            {
                return (byte?) message.Attributes.GetByteValue(GetPluginScopedName("Priority"));
            }

            return null;
        }
        
        public static void SetDateOccurred(this IMessage message, DateTime value) =>
            message.Attributes.SetDateValue(GetPluginScopedName("DateOccurred"), value, false);

        public static DateTime SetDateOccurred(this IMessage message) =>
            message.Attributes.GetDateTimeValue(GetPluginScopedName("DateOccurred"));
        
        public static void SetMessageId(this IMessage message, string value) =>
            message.Attributes.SetStringValue(value, GetPluginScopedName("MessageId"), false);
      

        public static string GetMessageId(this IMessage message) =>
            message.Attributes.GetStringValue(GetPluginScopedName("MessageId"), null);
        
        public static void SetSubject(this IMessage message, string value) =>
            message.Attributes.SetStringValue(value, GetPluginScopedName("Subject"), false);
      

        public static string GetSubject(this IMessage message) =>
            message.Attributes.GetStringValue(GetPluginScopedName("Subject"), null);
        
        public static void SetReplyTo(this IMessage message, string value) =>
            message.Attributes.SetStringValue(value, GetPluginScopedName("ReplyTo"), false);
      

        public static string GetReplyTo(this IMessage message) =>
            message.Attributes.GetStringValue(GetPluginScopedName("ReplyTo"), null);
        
        public static void SetTo(this IMessage message, string value) =>
            message.Attributes.SetStringValue(value, GetPluginScopedName("To"), false);
      

        public static string GetTo(this IMessage message) =>
            message.Attributes.GetStringValue(GetPluginScopedName("To"), null);

            
        public static void SetUserId(this IMessage message, string value) =>
            message.Attributes.SetStringValue(value, GetPluginScopedName("UserId"), false);
      

        public static string GetUserId(this IMessage message) =>
            message.Attributes.GetStringValue(GetPluginScopedName("UserId"), null);
        

        public static void SetAbsoluteExpiryTime(this IMessage message, DateTime value) =>
            message.Attributes.SetDateValue(GetPluginScopedName("AbsoluteExpiryTime"), value);


        public static DateTime? GetAbsoluteExpiryTime(this IMessage message)
        {
            if (message.Attributes.HasValue(GetPluginScopedName("AbsoluteExpiryTime")))
            {
                return message.Attributes.GetDateTimeValue(GetPluginScopedName("AbsoluteExpiryTime"));
            }

            return null;
        }
        
        public static void SetTls(this IMessage message, uint value) =>
            message.Attributes.SetUIntValue(GetPluginScopedName("Tls"), value);


        public static uint? GetTls(this IMessage message)
        {
            if (message.Attributes.HasValue(GetPluginScopedName("Tls")))
            {
                return message.Attributes.GetUIntValue(GetPluginScopedName("Tls"));
            }

            return null;
        }
    }
}