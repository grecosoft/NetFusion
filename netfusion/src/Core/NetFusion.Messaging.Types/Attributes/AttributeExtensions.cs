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
        public static string GetMessagingScopedName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name not specified", nameof(name));
            
            return $"NetFusion.Messaging.{name}";
        }

        /// <summary>
        /// Set a value used to identify the message if not already set.
        /// </summary>
        /// <param name="message">The message to set correlation value.</param>
        /// <param name="value">The value to identify the message.</param>
        public static void SetCorrelationId(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("CorrelationId"), value, false);
      
        /// <summary>
        /// Reads the value used to identify the message.  If not present, null is returned.
        /// </summary>
        /// <param name="message">The message to read correlation value.</param>
        /// <returns>The value used to identity the message.</returns>
        public static string GetCorrelationId(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("CorrelationId"), null);
       
        /// <summary>
        /// An arbitrary string value associated with the message.  Used to specify key
        /// used to determine which queues a message should be routed.
        /// </summary>
        /// <param name="message">The message to set route key.  If present, it
        /// will be overridden.</param>
        /// <param name="value">The arbitrary string value.</param>
        public static void SetRouteKey(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("RouteKey"), value);
        
        /// <summary>
        /// Given an array of values, sets the route key to the corresponding string
        /// values concatenated with period character.  Used to specify key used
        /// to determine which queues a message should be routed. 
        /// </summary>
        /// <param name="message">The message to set route key.  If present, it
        /// will be overridden.</param>
        /// <param name="values">Array of values to be encoded in the route key.</param>
        public static void SetRouteKey(this IMessage message, params object[] values)
        {
            string[] routeKeys = values.Where(v => v != null)
                .Select(v => v.ToString()).ToArray();

            message.Attributes.SetStringValue(GetMessagingScopedName("RouteKey"), string.Join(".", routeKeys));
        }

        /// <summary>
        /// Returns the route key specified for the message.
        /// </summary>
        /// <param name="message">The message to return associated route key.</param>
        /// <returns>The assigned route key value.  If not present, null is returned.</returns>
        public static string GetRouteKey(this IMessage message)
        {
            return message.Attributes.GetStringValue(GetMessagingScopedName("RouteKey"), null);
        }

        /// <summary>
        /// Set the priority value associated with the message in relation to other messages.
        /// </summary>
        /// <param name="message">The message to set priority.</param>
        /// <param name="value">The priority value.</param>
        public static void SetPriority(this IMessage message, byte value) =>
            message.Attributes.SetByteValue(GetMessagingScopedName("Priority"), value);

        /// <summary>
        /// Returns a priority value associated with the message in relation to other messages.
        /// </summary>
        /// <param name="message">The message to return associated priority.</param>
        /// <returns>The message's priority.</returns>
        public static byte? GetPriority(this IMessage message)
        {
            if (message.Attributes.HasValue(GetMessagingScopedName("Priority")))
            {
                return message.Attributes.GetByteValue(GetMessagingScopedName("Priority"));
            }

            return null;
        }
        
        /// <summary>
        /// The date and time the action represented by the message occurred.
        /// </summary>
        /// <param name="message">The message to set occurred date.</param>
        /// <param name="value">The date an time of the message.  If a value
        /// is already set, it will not be overridden.</param>
        public static void SetUtcDateOccurred(this IMessage message, DateTime value) =>
            message.Attributes.SetUtcDateValue(GetMessagingScopedName("DateOccurred"), value, false);

        /// <summary>
        /// The date and time the action represented by the message occurred.
        /// </summary>
        /// <param name="message">The message to retrieve the occurred date.</param>
        /// <returns></returns>
        public static DateTime GetUtcDateOccurred(this IMessage message) =>
            message.Attributes.GetUtcDateTimeValue(GetMessagingScopedName("DateOccurred"));

        /// <summary>
        /// Secondary value to identity the message.
        /// </summary>
        /// <param name="message">The message to set identity value.</param>
        /// <param name="value">A value used to identity the message.</param>
        public static void SetMessageId(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("MessageId"), value, false);
        
        /// <summary>
        /// Returns secondary value to identity the message.
        /// </summary>
        /// <param name="message">The message to retrieve identity value.</param>
        /// <returns>Value identifying the message.</returns>
        public static string GetMessageId(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("MessageId"), null);
        
        /// <summary>
        /// The content type used to serialize the message.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value"></param>
        public static void SetContentType(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("ContentType"), value);
        
        /// <summary>
        /// The content type used do serialize the message.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>The replay to value.</returns>
        public static string GetContentType(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("ContentType"), null);
        
        /// <summary>
        /// Message value that can be used to determine how a message is routed.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value">The current subject value.</param>
        public static void SetSubject(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("Subject"), value);

        /// <summary>
        /// Returns message value used to determine how a message is routed.
        /// </summary>
        /// <param name="message">The message to retrieve the attribute value.</param>
        /// <returns></returns>
        public static string GetSubject(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("Subject"), null);
        
        /// <summary>
        /// Message value indicating where the subscriber can replay to a received message.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value"></param>
        public static void SetReplyTo(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("ReplyTo"), value);
        
        /// <summary>
        /// Returns value indicating where the subscriber can replay to a received message.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>The replay to value.</returns>
        public static string GetReplyTo(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("ReplyTo"), null);
        
        /// <summary>
        /// Message value use to specify a destination for the message.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value">The destination value.</param>
        public static void SetTo(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("To"), value);
        
        /// <summary>
        /// Returns message value used to specify a destination for the message.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>Destination value.</returns>
        public static string GetTo(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("To"), null);
        
        /// <summary>
        /// Sets value used to identity a user associated with the message.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value">Value identity an user.</param>
        public static void SetUserId(this IMessage message, string value) =>
            message.Attributes.SetStringValue(GetMessagingScopedName("UserId"), value, false);
        
        /// <summary>
        /// Gets value used to identity user associated with the message.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>Value identifying an user.</returns>
        public static string GetUserId(this IMessage message) =>
            message.Attributes.GetStringValue(GetMessagingScopedName("UserId"), null);
        
        /// <summary>
        /// Sets date and time used to indicate after which the message should
        /// no longer be considered current and processed.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value">The expire date value.  The specified date will
        /// be converted to UTC is a local date is specified.</param>
        public static void SetUtcAbsoluteExpiryTime(this IMessage message, DateTime value) =>
            message.Attributes.SetUtcDateValue(GetMessagingScopedName("AbsoluteExpiryTime"), value);
        
        /// <summary>
        /// Get date an time used to indicate after which the message should
        /// no longer be considered current and processed.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>The expiry date and time.</returns>
        public static DateTime? GetUtcAbsoluteExpiryTime(this IMessage message)
        {
            if (message.Attributes.HasValue(GetMessagingScopedName("AbsoluteExpiryTime")))
            {
                return message.Attributes.GetUtcDateTimeValue(GetMessagingScopedName("AbsoluteExpiryTime"));
            }

            return null;
        }
        
        /// <summary>
        /// Sets token value for Transport Layer Security.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value">The security TLS token.</param>
        public static void SetTls(this IMessage message, uint value) =>
            message.Attributes.SetUIntValue(GetMessagingScopedName("Tls"), value);

        /// <summary>
        /// Returns token value for Transport Layer Security.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>The security TLS token.</returns>
        public static uint? GetTls(this IMessage message)
        {
            if (message.Attributes.HasValue(GetMessagingScopedName("Tls")))
            {
                return message.Attributes.GetUIntValue(GetMessagingScopedName("Tls"));
            }

            return null;
        }

        /// <summary>
        /// Used to specify a time after which a message should no longer be processed.
        /// </summary>
        /// <param name="message">The message to set attribute value.</param>
        /// <param name="value">The timespan value.</param>
        public static void SetTimeToLive(this IMessage message, TimeSpan value) =>
            message.Attributes.SetTimeSpan(GetMessagingScopedName("TimeToLive"), value);

        /// <summary>
        /// Returns the time after which a message should no longer be processed.
        /// </summary>
        /// <param name="message">The message to retrieve attribute value.</param>
        /// <returns>The timespan value.</returns>
        public static TimeSpan? GetTimeToLive(this IMessage message)
        {
            if (message.Attributes.HasValue(GetMessagingScopedName("TimeToLive")))
            {
                return message.Attributes.GetTimeSpanValue(GetMessagingScopedName("TimeToLive"));
            }

            return null;
        }
    }
}