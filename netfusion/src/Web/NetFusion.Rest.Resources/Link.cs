namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Defines a link that can be associated with a resource.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The hypertext reference to the associated resource.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Its value is a string and is intended for indicating the language of
        /// the target resource(as defined by [RFC5988]) (Optional).
        /// </summary>
        public string HrefLang { get; set; }

        /// <summary>
        /// Indicates if the HREF value is a template (Optional).
        /// </summary>
        public bool? Templated { get; set; }

        /// <summary>
        /// Indicates the methods that can be used to call the HREF.
        /// </summary>
        public string[] Methods { get; set; }

        /// <summary>
        ///  Its value MAY be used as a secondary key for selecting Link Objects which 
        ///  share the same relation type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Its value is a string and is intended for labeling the link with a
        /// human-readable identifier(as defined by [RFC5988]) (Optional).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Its value is a string used as a hint to indicate the media type
        /// expected when dereferencing the target resource (Optional).
        /// </summary>
        public string Type { get; set; }
    }
}
