using System;
using System.Linq;
using NetFusion.Messaging.Types.Contracts;

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
        private static readonly Type Context = typeof(MessagingContext);

        /// <summary>
        /// Set a value used to identify the message if not already set.
        /// </summary>
        /// <param name="message">The message to set correlation value.</param>
        /// <param name="value">The value to identify the message.</param>
        public static void SetCorrelationId(this IMessage message, string value) =>
            message.Attributes.SetMemberValue(value, Context, overrideIfPresent: false);
      
        /// <summary>
        /// Reads the value used to identify the message.  If not present, null is returned.
        /// </summary>
        /// <param name="message">The message to read correlation value.</param>
        /// <returns>The value used to identity the message.</returns>
        public static string GetCorrelationId(this IMessage message) =>
            message.Attributes.GetMemberValueOrDefault<string>(null, Context);
       
        /// <summary>
        /// An arbitrary string value associated with the message.
        /// </summary>
        /// <param name="message">The message to set route key.  If present, it
        /// will be overriden.</param>
        /// <param name="value">The arbitrary string value.</param>
        public static void SetRouteKey(this IMessage message, string value) =>
            message.Attributes.SetMemberValue(value, Context);
        

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

            message.Attributes.SetMemberValue(string.Join(".", routeKeys), Context);
        }

        /// <summary>
        /// Returns the route key specified for the message.
        /// </summary>
        /// <param name="message">The message to return associated route key.</param>
        /// <returns>The assigned route key value.  If not present, null is returned.</returns>
        public static string GetRouteKey(this IMessage message)
        {
            return message.Attributes.GetMemberValueOrDefault<string>(null, Context);
        }

        /// <summary>
        /// Set the priority value associated with the message in relation to other messages.
        /// </summary>
        /// <param name="message">The message to set priority.</param>
        /// <param name="value">The priority value.</param>
        public static void SetPriority(this IMessage message, byte value) =>
            message.Attributes.SetMemberValue(value, Context);

        /// <summary>
        /// Returns a priority value associated with the message in relation to other messages.
        /// </summary>
        /// <param name="message">The message to return associated priority.</param>
        /// <returns>The message's priority.</returns>
        public static byte? GetPriority(this IMessage message) =>
            message.Attributes.GetValueOrDefault<byte?>("Priority", null, Context);
    }
}
