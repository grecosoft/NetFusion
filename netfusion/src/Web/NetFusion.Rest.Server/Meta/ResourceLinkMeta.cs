using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using NetFusion.Rest.Server.Linking;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Contains methods creating ResourceLink instances and associating metadata.
    /// </summary>
    /// <typeparam name="TSource">The source type associated with metadata.</typeparam>
    public class ResourceLinkMeta<TSource>
        where TSource : class
    {
        private readonly List<ResourceLink> _resourceLinks = new List<ResourceLink>();
      
        /// <summary>
        /// Returns the ResourceLink instances populated with link metadata.
        /// </summary>
        /// <returns>Instance of the created ResourceLinks.</returns>
        internal ResourceLink[] GetResourceLinks() => _resourceLinks.ToArray();

        protected void AddResourceLink(ResourceLink resourceLink)
        {
            if (resourceLink == null) throw new ArgumentNullException(nameof(resourceLink));
            
            _resourceLinks.Add(resourceLink);
        }
        
        //--------- STRING BASED LINKS ------------------------------------------

        /// <summary>
        /// Creates a named link relation for a hard-coded URI value.
        /// </summary>
        /// <param name="relName">The relation name.</param>
        /// <param name="href">The URI associated with the relation.</param>
        /// <param name="httpMethod">The HTTP method used to invoke the URI.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> Href(string relName, HttpMethod httpMethod, string href)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException("Relation Name not specified.", nameof(relName));
            if (string.IsNullOrWhiteSpace(href)) throw new ArgumentException("HREF value not specified.", nameof(href));

            var resourceLink = new ResourceLink();
            var linkDescriptor = new LinkDescriptor<TSource>(resourceLink);

            AddResourceLink(resourceLink);

            linkDescriptor.SetRelName(relName);
            linkDescriptor.SetHref(href);
            linkDescriptor.SetMethod(httpMethod);

            return linkDescriptor;
        }

        /// <summary>
        /// Create a named link based on string interpolation based on a resource's model properties. 
        /// </summary>
        /// <param name="relName">The relation name.</param>
        /// <param name="httpMethod">The HTTP method used to invoke URI.</param>
        /// <param name="resourceUrl">Function delegate passed the model during link resolution and
        /// returns a populated URI or partial populated URI (i.e. Template) based on the properties
        /// of the passed resource.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> Href(string relName, HttpMethod httpMethod, Expression<Func<TSource, string>> resourceUrl)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException(
                "Relation Name not specified.", nameof(relName));

            if (resourceUrl == null) throw new ArgumentNullException(nameof(resourceUrl), 
                "Resource Url Delegate cannot be null.");

            var resourceLink = new InterpolatedLink<TSource>(resourceUrl);
            var linkDescriptor = new LinkDescriptor<TSource>(resourceLink);

            AddResourceLink(resourceLink);

            linkDescriptor.SetRelName(relName);
            linkDescriptor.SetMethod(httpMethod);
            return linkDescriptor;
        }
    }
    
    
    //--------- CONTROLLER/ACTION BASED LINKS ------------------------------------------
    

    /// <summary>
    /// Contains methods for creating ResourceLink instances based on expressions for a given
    /// controller and resource's associated model type.
    /// </summary>
    /// <typeparam name="TController">The controller type associated with the link metadata.</typeparam>
    /// <typeparam name="TSource">The source type associated with the link metadata.</typeparam>
    public class ResourceLinkMeta<TController, TSource> : ResourceLinkMeta<TSource>
        where TController : ControllerBase
        where TSource: class
    {
        /// <summary>
        /// Used to specify a fully populated link associated with a specified relation name. 
        /// </summary>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method and the
        /// model's properties that should be mapped to the action method arguments used during link resolution.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> Url(string relName,
            Expression<Action<TController, TSource>> action)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException(
                "Relation Name not specified.", nameof(relName));

            if (action == null) throw new ArgumentNullException(nameof(action),
                "Controller Action selector cannot be null.");

            var resourceLink = new ControllerActionLink();
            var actionSelector = new ControllerActionRouteSelector<TController, TSource>(resourceLink, action);          
            var linkDescriptor = new LinkDescriptor<TSource>(resourceLink);

            AddResourceLink(resourceLink);

            linkDescriptor.SetRelName(relName);
            actionSelector.SetRouteInfo();

            return linkDescriptor;
        }
        
        
        //--------- CONTROLLER/ACTION TEMPLATE BASED LINKS ------------------------------------------
   
        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking zero arguments.
        /// </summary>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> UrlTemplate<TResponse>(string relName, 
            Expression<Func<TController, Func<TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking one argument.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument type.</typeparam>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> UrlTemplate<T1, TResponse>(string relName, 
            Expression<Func<TController, Func<T1, TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking two arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument type.</typeparam>
        /// <typeparam name="T2">The type of the second argument type.</typeparam>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> UrlTemplate<T1, T2, TResponse>(string relName, 
            Expression<Func<TController, Func<T1, T2, TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking three arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument type.</typeparam>
        /// <typeparam name="T2">The type of the second argument type.</typeparam>
        /// <typeparam name="T3">The type of the third argument type.</typeparam>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> UrlTemplate<T1, T2, T3, TResponse>(string relName, 
            Expression<Func<TController, Func<T1, T2, T3, TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking four arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument type.</typeparam>
        /// <typeparam name="T2">The type of the second argument type.</typeparam>
        /// <typeparam name="T3">The type of the third argument type.</typeparam>
        /// <typeparam name="T4">The type of the forth argument type.</typeparam>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> UrlTemplate<T1, T2, T3, T4, TResponse>(string relName, 
            Expression<Func<TController, Func<T1, T2, T3, T4, TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking five arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument type.</typeparam>
        /// <typeparam name="T2">The type of the second argument type.</typeparam>
        /// <typeparam name="T3">The type of the third argument type.</typeparam>
        /// <typeparam name="T4">The type of the forth argument type.</typeparam>
        /// <typeparam name="T5">The type of the fifth argument type.</typeparam>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TSource> UrlTemplate<T1, T2, T3, T4, T5, TResponse>(string relName, 
            Expression<Func<TController, Func<T1, T2, T3, T4, T5, TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        private LinkDescriptor<TSource> SetUrlTemplateMetadata(string relName, LambdaExpression action)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException(
                "Relation Name not specified.", nameof(relName));

            if (action == null) throw new ArgumentNullException(nameof(action), 
                "Controller Action selector cannot be null.");

            var resourceLink = new TemplateUrlLink();
            var actionSelector = new ControllerActionTemplateSelector<TController>(resourceLink, action);
            var linkDescriptor = new LinkDescriptor<TSource>(resourceLink);

            AddResourceLink(resourceLink);

            linkDescriptor.SetRelName(relName);
            actionSelector.SetTemplateInfo();
            return linkDescriptor;
        }
    }
}
