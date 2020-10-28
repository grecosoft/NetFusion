namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Property containing a Name/Value pair recording details associated
    /// with the log message.  When a log is written, these properties are
    /// added as details to the created structured log event.
    /// </summary>
    public class LogProperty
    {
        /// <summary>
        /// Name identifying the log value.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The value of the log.
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// If true, and the value is a non-primitive, non-array type, then the value will be converted
        /// to a structure; otherwise, unknown types will be converted to scalars, which are generally
        /// stored as strings.
        /// </summary>
        public bool DestructureObjects { get; set; }
    }
}