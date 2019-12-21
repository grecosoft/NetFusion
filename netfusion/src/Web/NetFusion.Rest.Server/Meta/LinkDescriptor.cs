using System;
using System.Net.Http;
using NetFusion.Rest.Server.Linking;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Instance of class used to specify required and optional link metadata properties.
    /// </summary>
    /// <typeparam name="TResource">The resource associated with the metadata.</typeparam>
    public class LinkDescriptor<TResource>
        where TResource : class
    {
        private readonly ResourceLink _resourceLink;

        public LinkDescriptor(ResourceLink resourceLink)
        {
            _resourceLink = resourceLink ?? throw new ArgumentNullException(nameof(resourceLink),
                "Action Link cannot be null.");
        }

        //---------------- REQUIRED LINK PROPERTIES SET BY LINK METADATA METHODS ----------------------

        internal LinkDescriptor<TResource> SetRelName(string relName)
        {
            if (string.IsNullOrWhiteSpace(relName))
                throw new ArgumentException("Relation Name not specified.", nameof(relName));

            _resourceLink.RelationName = relName;
            return this;
        }

        internal LinkDescriptor<TResource> SetMethod(HttpMethod method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method), 
                "HTTP Method cannot be null.");

            _resourceLink.Methods = new[] { method.Method };

            return this;
        }

        internal LinkDescriptor<TResource> SetHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                throw new ArgumentException("Href Value not specified.", nameof(href));

            _resourceLink.Href = href;
            return this;
        }

        //---------------- OPTIONAL LINK PROPERTIES SET BY RESOURCE MAPPINGS ----------------------

        public LinkDescriptor<TResource> SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name Value not specified.", nameof(name));

            _resourceLink.Name = name;
            return this;
        }

        public LinkDescriptor<TResource> SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title Value not specified.", nameof(title));

            _resourceLink.Title = title;
            return this;
        }

        public LinkDescriptor<TResource> SetType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type Value not specified.", nameof(type));

            _resourceLink.Type = type;
            return this;
        }

        public LinkDescriptor<TResource> SetHrefLang(string hrefLang)
        {
            if (string.IsNullOrWhiteSpace(hrefLang))
                throw new ArgumentException("HrefLang Value not specified.", nameof(hrefLang));

            _resourceLink.HrefLang = hrefLang;
            return this;
        }
    }
}
