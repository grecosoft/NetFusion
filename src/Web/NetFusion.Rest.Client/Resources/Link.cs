namespace NetFusion.Rest.Client.Resources
{
    /// <summary>
    /// Representation of REST Link for consuming client returned
    /// from server API.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The URL for the resource.
        /// </summary>
		public string Href { get; set; }

        /// <summary>
        /// The methods supported by the resource URL.
        /// </summary>
        public string[] Methods { get; set; }

		/// <summary>
		/// Indicates if the URL contains template value place holders.
		/// </summary>
		public bool Templated { get; set; }

		/// <summary>
		/// Its value is a string and is intended for indicating the language of
		/// the target resource(as defined by [RFC5988]) (Optional).
		/// </summary>
		public string HrefLang { get; set; }

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

        /// A client SHOULD provide some notification (for example, by logging a
        /// warning message) whenever it traverses over a link that has this
        /// property.The notification SHOULD include the deprecation property's
        /// value so that a client maintainer can easily find information about
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

