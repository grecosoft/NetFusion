namespace NetFusion.Base
{
    /// <summary>
    /// Serialization content-types supported by default.
    /// </summary>
    public static class SerializerTypes
    {
        public const string Json = "application/json; charset=utf-8";
        public const string Binary = "application/octet-stream";
        public const string MessagePack = "application/x-msgpack";
    }
}
