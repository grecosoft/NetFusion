namespace NetFusion.RabbitMQ.Settings
{
    /// <summary>
    /// Contains default constant values that are used if not explicitly specified.
    /// </summary>
    public static class SettingDefaults
    {
        /// <summary>
        /// The number of milliseconds after which a RPC published message will timeout.
        /// </summary>
        public const int RpcTimeOutAfterMs = 10_000;
    }
}