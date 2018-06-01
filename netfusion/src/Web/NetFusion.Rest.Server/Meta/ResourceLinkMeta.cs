using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Common;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Actions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Contains methods creating ActionLink instances and associating metadata.
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    public class ResourceLinkMeta<TResource>
        where TResource : class, IResource
    {
        private List<ActionLink> _actionLinks = new List<ActionLink>();
      
        /// <summary>
        /// Returns the ActionLink instance populated with link metadata.
        /// </summary>
        /// <returns>Instance of the created ActionLink.</returns>
        internal ActionLink[] GetActionLinks() => _actionLinks.ToArray();

        protected void AddActionLink(ActionLink actionLink)
        {
            _actionLinks.Add(actionLink);
        }

        /// <summary>
        /// Creates a named link relation for a hard-coded URI value.
        /// </summary>
        /// <param name="relName">The relation name.</param>
        /// <param name="href">The URI associated with the relation.</param>
        /// <param name="httpMethod">The HTTP method used to invoke the URI.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TResource> Href(string relName, HttpMethod httpMethod, string href)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException("Relation Name not specified.", nameof(relName));
            if (string.IsNullOrWhiteSpace(href)) throw new ArgumentException("HREF value not specified.", nameof(href));

            var actionLink = new ActionLink();
            var linkDescriptor = new LinkDescriptor<TResource>(actionLink);

            AddActionLink(actionLink);

            linkDescriptor.SetRelName(relName);
            linkDescriptor.SetHref(href);
            linkDescriptor.SetMethod(httpMethod);

            return linkDescriptor;
        }

        /// <summary>
        /// Create a named link related for a parametrized URI containing place holds based on
        /// resource properties to be substituted at link resolution time. 
        /// </summary>
        /// <param name="relName">The relation name.</param>
        /// <param name="httpMethod">The HTTP method used to invoke URI.</param>
        /// <param name="resourceUrl">Function delegate passed the resource during link resolution and
        /// returns a populated URI or partial populated URI (i.e. Template) based on the properties
        /// of the passed resourced.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TResource> Href(string relName, HttpMethod httpMethod, Expression<Func<TResource, string>> resourceUrl)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException(
                "Relation Name not specified.", nameof(relName));

            if (resourceUrl == null) throw new ArgumentNullException(nameof(resourceUrl), 
                "Resource Delegate cannot be null.");

            var actionLink = new ActionResourceLink<TResource>(resourceUrl);
            var linkDescriptor = new LinkDescriptor<TResource>(actionLink);

            AddActionLink(actionLink);

            linkDescriptor.SetRelName(relName);
            linkDescriptor.SetMethod(httpMethod);
            return linkDescriptor;
        }
    }

    /// <summary>
    ///  Contains methods for creating ActionLink instances based on expressions for a given controller and resource type.
    /// </summary>
    /// <typeparam name="TController">The controller type associated with the link metadata.</typeparam>
    /// <typeparam name="TResource">The resource type associated with the link metadata.</typeparam>
    public class ResourceLinkMeta<TController, TResource> : ResourceLinkMeta<TResource>
        where TController : Controller
        where TResource: class, IResource
    {
        /// <summary>
        /// Used to specify a fully populated link associated with a specified relation name. 
        /// </summary>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method and 
        /// the resource properties that should be mapped to the action method arguments used during link resolution.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TResource> Url(string relName,
            Expression<Action<TController, TResource>> action)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException(
                "Relation Name not specified.", nameof(relName));

            if (action == null) throw new ArgumentNullException(nameof(action),
                "Controller Action selector cannot be null.");

            var actionLink = new ActionUrlLink();
            var actionSelector = new ActionUrlSelector<TController, TResource>(actionLink, action);          
            var linkDescriptor = new LinkDescriptor<TResource>(actionLink);

            AddActionLink(actionLink);

            linkDescriptor.SetRelName(relName);
            actionSelector.SetRouteInfo();

            return linkDescriptor;
        }

        /// <summary>
        /// Based on the specified resource and controller, will determine if a controller action method exists,
        /// based on conventions, corresponding to the action to be invoked to load the current state of the resource.
        /// </summary>
        /// <param name="relName">The relation name to use.  Defaults to Self.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
		public void AutoMapSelfRelation(string relName = RelationTypes.Self)
		{
            var querySelector = new ActionSelfSelector(typeof(TController), typeof(TResource));
            var actionLink = querySelector.FindSelfResourceAction();

            if (actionLink != null)
            {
                actionLink.RelationName = relName;
                AddActionLink(actionLink);
            }
		}

        /// <summary>
        /// Based on the specified resource and controller, will determine if a controller action methods exist,
        /// based on conventions, corresponding to the action to be invoked for creating, updating, and deleting
        /// the resource.
        /// </summary>
        public void AutoMapUpdateRelations()
        {
            var updatedSelector = new ActionUpdateSelector(typeof(TController), typeof(TResource));

            var actionLink = updatedSelector.FindCreateResourceAction();
            if (actionLink != null)
            {
                actionLink.RelationName = "resource:create";
                AddActionLink(actionLink);
            }

            actionLink = updatedSelector.FindUpdateResourceAction();
            if (actionLink != null)
            {
                actionLink.RelationName = "resource:update";
                AddActionLink(actionLink);
            }

            actionLink = updatedSelector.FindDeleteResourceAction();
            if (actionLink != null)
            {
                actionLink.RelationName = "resource:delete";
                AddActionLink(actionLink);
            }
        }

        /// <summary>
        /// Used to specify a link template associated with a specified relation name for a controller
        /// action method taking zero arguments.
        /// </summary>
        /// <typeparam name="TResponse">The response returned by selected controller's action.</typeparam>
        /// <param name="relName">The relation name.</param>
        /// <param name="action">Controller type metadata used to select a controller's action method containing
        /// URI route tokens to be populated by the consuming client.</param>
        /// <returns>Object used to add optional metadata to the created link.</returns>
        public LinkDescriptor<TResource> UrlTemplate<TResponse>(string relName, 
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
        public LinkDescriptor<TResource> UrlTemplate<T1, TResponse>(string relName, 
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
        public LinkDescriptor<TResource> UrlTemplate<T1, T2, TResponse>(string relName, 
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
        public LinkDescriptor<TResource> UrlTemplate<T1, T2, T3, TResponse>(string relName, 
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
        public LinkDescriptor<TResource> UrlTemplate<T1, T2, T3, T4, TResponse>(string relName, 
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
        public LinkDescriptor<TResource> UrlTemplate<T1, T2, T3, T4, T5, TResponse>(string relName, 
            Expression<Func<TController, Func<T1, T2, T3, T4, T5, TResponse>>> action)
        {
            return SetUrlTemplateMetadata(relName, action);
        }

        private LinkDescriptor<TResource> SetUrlTemplateMetadata(string relName, LambdaExpression action)
        {
            if (string.IsNullOrWhiteSpace(relName)) throw new ArgumentException(
                "Relation Name not specified.", nameof(relName));

            if (action == null) throw new ArgumentNullException(nameof(action), 
                "Controller Action selector cannot be null.");

            var actionLink = new ActionTemplateLink();
            var actionSelector = new ActionTemplateSelector<TController>(actionLink, action);
            var linkDescriptor = new LinkDescriptor<TResource>(actionLink);

            AddActionLink(actionLink);

            linkDescriptor.SetRelName(relName);
            actionSelector.SetTemplateInfo();
            return linkDescriptor;
        }
    }
}
