using System;
using System.Net.Http;
using NetFusion.Rest.Server.Linking;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Instance of class used to specify required and optional link metadata properties.
    /// </summary>
    /// <typeparam name="TSource">The source type associated with the metadata.</typeparam>
    public class LinkDescriptor<TSource>
        where TSource : class
    {
        private readonly ResourceLink _resourceLink;

        public LinkDescriptor(ResourceLink resourceLink)
        {
            _resourceLink = resourceLink ?? throw new ArgumentNullException(nameof(resourceLink),
                "Resource Link cannot be null.");
        }

        //---------------- REQUIRED LINK PROPERTIES SET BY LINK METADATA METHODS ----------------------

        internal LinkDescriptor<TSource> SetRelName(string relName)
        {
            if (string.IsNullOrWhiteSpace(relName))
                throw new ArgumentException("Relation Name not specified.", nameof(relName));

            _resourceLink.RelationName = relName;
            return this;
        }

        internal LinkDescriptor<TSource> SetMethod(HttpMethod method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method), 
                "HTTP Method cannot be null.");

            _resourceLink.Method = method.Method;

            return this;
        }

        internal LinkDescriptor<TSource> SetHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                throw new ArgumentException("Href Value not specified.", nameof(href));

            _resourceLink.Href = href;
            return this;
        }

        //---------------- OPTIONAL LINK PROPERTIES SET BY RESOURCE MAPPINGS ----------------------

        public LinkDescriptor<TSource> SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name Value not specified.", nameof(name));

            _resourceLink.Name = name;
            return this;
        }

        public LinkDescriptor<TSource> SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title Value not specified.", nameof(title));

            _resourceLink.Title = title;
            return this;
        }

        public LinkDescriptor<TSource> SetType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type Value not specified.", nameof(type));

            _resourceLink.Type = type;
            return this;
        }

        public LinkDescriptor<TSource> SetHrefLang(string hrefLang)
        {
            if (string.IsNullOrWhiteSpace(hrefLang))
                throw new ArgumentException("HrefLang Value not specified.", nameof(hrefLang));

            _resourceLink.HrefLang = hrefLang;
            return this;
        }
    }
}
