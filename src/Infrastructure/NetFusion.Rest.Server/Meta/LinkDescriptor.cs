using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Actions;
using NetFusion.Rest.Web.Actions;
using System;
using System.Linq.Expressions;
using System.Net.Http;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Instance of class used to specify required and optional link metadata properties.
    /// </summary>
    /// <typeparam name="TResource">The resource associated with the metadata.</typeparam>
    public class LinkDescriptor<TResource>
        where TResource : class, IResource
    {
        private ActionLink _actionLink;

        public LinkDescriptor(ActionLink actionLink)
        {
            _actionLink = actionLink
                ?? throw new ArgumentNullException(nameof(actionLink), "Action Link not specified.");
        }

        //---------------- REQUIRED LINK PROPERTIES SET BY LINK METADATA METHODS ----------------------

        internal LinkDescriptor<TResource> SetRelName(string relName)
        {
            if (string.IsNullOrWhiteSpace(relName))
                throw new ArgumentException("Relation Name not specified.", nameof(relName));

            _actionLink.RelationName = relName;
            return this;
        }

        internal LinkDescriptor<TResource> SetMethod(HttpMethod method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method), "HTTP Method not specified.");

            _actionLink.Methods = new[] { method.Method };

            return this;
        }

        internal LinkDescriptor<TResource> SetHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                throw new ArgumentException("Href Value not specified.", nameof(href));

            _actionLink.Href = href;
            return this;
        }

        //---------------- OPTIONAL LINK PROPERTIES SET BY RESOURCE MAPPINGS ----------------------

        public LinkDescriptor<TResource> SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name Value not specified.", nameof(name));

            _actionLink.Name = name;
            return this;
        }

        public LinkDescriptor<TResource> SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title Value not specified.", nameof(title));

            _actionLink.Title = title;
            return this;
        }

        public LinkDescriptor<TResource> SetType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type Value not specified.", nameof(type));

            _actionLink.Type = type;
            return this;
        }

        public LinkDescriptor<TResource> SetHrefLang(string hrefLang)
        {
            if (string.IsNullOrWhiteSpace(hrefLang))
                throw new ArgumentException("HrefLang Value not specified.", nameof(hrefLang));

            _actionLink.HrefLang = hrefLang;
            return this;
        }

        //---------------- OPTIONAL DEPRECATION LINK PROPERTY GENERATION  ----------------------

        public LinkDescriptor<TResource> SetDeprecation<TController>(Expression<Action<TController, TResource>> action)
            where TController : Controller
        {
            _actionLink.Deprecation = BuildActionUrlLink(action);
            return new LinkDescriptor<TResource>(_actionLink.Deprecation);
        }

        public LinkDescriptor<TResource> SetDeprecation(HttpMethod httpMethod, Expression<Func<TResource, string>> resourceUrl)           
        {
            _actionLink.Deprecation = BuildResourceUrl(httpMethod, resourceUrl);
            return new LinkDescriptor<TResource>(_actionLink.Deprecation);
        }

        public LinkDescriptor<TResource> SetDeprecation(HttpMethod httpMethod, string href)
        {
            _actionLink.Deprecation = BuildUrl(httpMethod, href);
            return new LinkDescriptor<TResource>(_actionLink.Deprecation);
        }

        //---------------- OPTIONAL PROFILE LINK PROPERTY GENERATION  ----------------------

        public LinkDescriptor<TResource> SetProfile<TController>(Expression<Action<TController, TResource>> action)
            where TController : Controller
        {
            _actionLink.Profile = BuildActionUrlLink(action);
            return new LinkDescriptor<TResource>(_actionLink.Profile);
        }

        public LinkDescriptor<TResource> SetProfile(HttpMethod httpMethod, Expression<Func<TResource, string>> resourceUrl)
        {
            _actionLink.Profile = BuildResourceUrl(httpMethod, resourceUrl);
            return new LinkDescriptor<TResource>(_actionLink.Profile);
        }

        public LinkDescriptor<TResource> SetProfile(HttpMethod httpMethod, string href)
        {
            _actionLink.Profile = BuildUrl(httpMethod, href);
            return new LinkDescriptor<TResource>(_actionLink.Profile);
        }


        //-------------------- COMMON LINK PROPERTY GENERATION METHODS ----------------------

        public ActionUrlLink BuildActionUrlLink<TController>(Expression<Action<TController, TResource>> action)
            where TController : Controller
        {
            if (action == null) throw new ArgumentNullException(nameof(action), "Controller Action selector not specified.");

            var actionLink = new ActionUrlLink();
            var actionSelector = new ActionUrlSelector<TController, TResource>(actionLink, action);

            actionSelector.SetRouteInfo();
            return actionLink;
        }

        public ActionResourceLink<TResource> BuildResourceUrl(HttpMethod httpMethod, Expression<Func<TResource, string>> resourceUrl)
        {
            if (resourceUrl == null) throw new ArgumentNullException(nameof(resourceUrl), "Resource Delegate not specified.");

            var actionLink = new ActionResourceLink<TResource>(resourceUrl);
            var linkDescriptor = new LinkDescriptor<TResource>(actionLink);

            linkDescriptor.SetMethod(httpMethod);
            return actionLink;
        }

        public ActionLink BuildUrl(HttpMethod httpMethod, string href)
        {
            if (string.IsNullOrWhiteSpace(href)) throw new ArgumentException("HREF value not specified.", nameof(href));

            var actionLink = new ActionLink();
            var linkDescriptor = new LinkDescriptor<TResource>(actionLink);

            linkDescriptor.SetHref(href);
            linkDescriptor.SetMethod(httpMethod);
            return actionLink;
        }
    }
}
