namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Properties associated with a RPC style request.
    /// </summary>
    public class RpcProperties
    {
        /// <summary>
        /// A string value identifying the type of the published command.
        /// This can be used by the consumer to map to a type.
        /// </summary>
        public string ExternalTypeName { get; set; }

        /// <summary>
        /// The content type of the serialized command message.
        /// </summary>
        public string ContentType { get; set; }
    }
}
