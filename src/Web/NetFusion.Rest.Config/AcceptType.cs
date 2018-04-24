namespace NetFusion.Rest.Config
{
    /// <summary>
    /// Configuration setting values for the accept header.
    /// </summary>
    public class AcceptType
    {
        /// <summary>
        /// The media type to accept.
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// Value used to specify the priority of the accept value
        /// compared to others.
        /// </summary>
        public double? Quality { get; set; }
    }
}
