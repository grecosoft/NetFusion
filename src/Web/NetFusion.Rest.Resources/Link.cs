namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Defines a link with an associated relation name.  The relation
    /// name indicates how the link is associated to the resource. The
    /// link is populated based on a resource and its corresponding 
    /// metadata contained within the resource-mappings.
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

        /// <summary>
        /// Its presence indicates that the link is to be deprecated (i.e removed) 
        /// at a future date.  Its value is a URL that SHOULD provide further information 
        /// about the deprecation.
        /// 
        /// A client SHOULD provide some notification (for example, by logging a
        /// warning message) whenever it traverses over a link that has this
        /// property.The notification SHOULD include the deprecation property's
        /// value so that a client implementor can easily find information about
        /// the deprecation (Optional).
        /// </summary>
        public Link Deprecation { get; set; }

        /// <summary>
        /// Its value is a string which is a URI that hints about the profile (as
        /// defined by[I - D.wilde - profile - link]) of the target resource (Optional).
        /// </summary>
        public Link Profile { get; set; }
    }
}
