using System.Text;

namespace NetFusion.Integration.Redis.Internal
{
    /// <summary>
    /// Encodes and Decodes a byte array containing parts used to identify the following:
    ///
    /// - The length of the content type (Encoded as a UTF8 string)
    /// - The Encoded UTF8 string
    /// - The serialized message as the specified content-type
    ///
    /// When publishing and consuming a message in Redis, there is no a concept of a header
    /// that would be normally used to specify the content-type.  Encoding and Decoding this
    /// data in the message byte array allows the publisher and consumer to be completely
    /// decoupled.  This allows the consumer to deserialize the message using the content-type
    /// specified by the publisher - allowing the content-type to change over time without having
    /// to recompile the consumer. 
    /// </summary>
    public static class ChannelMessageEncoder
    {
        /// <summary>
        /// Returns byte array containing content type and serialized message.
        /// </summary>
        /// <param name="contentType">The content-type of the serialized message.</param>
        /// <param name="messageData">The serialized message.</param>
        /// <returns>Byte array containing content type and message data.</returns>
        public static byte[] Pack(string contentType, byte[] messageData)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content-Type not specified.", nameof(contentType));
            
            if (messageData == null) throw new ArgumentNullException(nameof(messageData));
            
            byte[] contentTypeData = Encoding.UTF8.GetBytes(contentType);
            
            byte[][] data = {
                BitConverter.GetBytes(contentTypeData.Length),
                contentTypeData,
                messageData
            };

            return data.SelectMany(i => i).ToArray();
        }

        /// <summary>
        /// Returns the contents encoded within packed channel message.
        /// </summary>
        /// <param name="value">The packed channel message.</param>
        /// <returns>The content-type and the message data.</returns>
        public static (string contentType, byte[] messageData) UnPack(byte[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            int contentTypeLen = BitConverter.ToInt32(value, 0);
            byte[] contentTypeData = value.Skip(sizeof(int)).Take(contentTypeLen).ToArray();

            string contentType = Encoding.UTF8.GetString(contentTypeData);
            byte[] messageData = value.Skip(sizeof(int) + contentTypeLen).ToArray();

            return (contentType, messageData);
        }
    }
}