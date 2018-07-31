namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Contains default constant values that are used if
    /// not explicitly specified.
    /// </summary>
    public static class SettingDefaults
    {
        /// <summary>
        /// The name of the default bus if not specified in code.
        /// </summary>
        public const string DefaultBusName = "message-bus";

        /// <summary>
        /// The number of milliseconds after which a RPC published
        /// message will timeout.
        /// </summary>
        public const int RpcTimeOutAfterMs = 10_000;
    }
}