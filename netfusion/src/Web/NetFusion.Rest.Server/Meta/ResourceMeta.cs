﻿using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Actions;
using NetFusion.Rest.Server.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Base resource metadata associated with resource.
    /// </summary>
    /// <typeparam name="TResourceMeta">Derived metadata class containing specific metadata.  This is the type
    /// returned from the fluent methods.  This allows method chaining to be applied based on the derived type.
    /// </typeparam>
    /// <typeparam name="TResource">The type of resource associated with the metadata.</typeparam>
    public class ResourceMeta<TResourceMeta, TResource> : IResourceMeta
	    where TResource : class, IResource
		where TResourceMeta : ResourceMeta<TResourceMeta, TResource>
    {
        public Type ResourceType => typeof(TResource);
       
        /// <summary>
        /// Links associated with the resource.  Usually used to navigate 
        /// additional associated resources.
        /// </summary>
        public IReadOnlyCollection<ActionLink> Links { get; }

        private readonly List<ActionLink> _links = new List<ActionLink>();
        private readonly IResourceMap _resourceMap;

        public ResourceMeta(IResourceMap resourceMap)
        {
            Links = _links.AsReadOnly();

            _resourceMap = resourceMap ?? throw new ArgumentNullException(nameof(resourceMap));
        }

        /// <summary>
        /// Returns link metadata class used to specify controller action methods.
        /// </summary>
        /// <typeparam name="TController">The controller type so select action method from.</typeparam>
        /// <param name="meta">Method delegate passed metadata class used to define link metadata.</param>
        /// <returns>Reference to self for method chaining.</returns>
        public TResourceMeta LinkMeta<TController>(Action<ResourceLinkMeta<TController, TResource>> meta)
            where TController : ControllerBase
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta),
                "Metadata delegate cannot be null.");

            AssertControllerMeetsConstraints(typeof(TController));

            var resourceLinkMeta = new ResourceLinkMeta<TController, TResource>();
            meta(resourceLinkMeta);

            AddLinks(resourceLinkMeta.GetActionLinks());
            return (TResourceMeta)this;
        }

        /// <summary>
        /// Returns like metadata class used to specify URIs not associated with a specific controller method.
        /// </summary>
        /// <param name="meta">Method delegate passed metadata class used to define like metadata.</param>
        /// <returns>Reference to self for method chaining.</returns>
        public TResourceMeta LinkMeta(Action<ResourceLinkMeta<TResource>> meta)
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta),
                "Metadata delegate not specified.");

            var resourceLinkMeta = new ResourceLinkMeta<TResource>();
            meta(resourceLinkMeta);

            AddLinks(resourceLinkMeta.GetActionLinks());
            return (TResourceMeta)this;
        }

        protected void AddLinks(ActionLink[] links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links), "Reference to link array cannot be null.");
            }
            _links.AddRange(links);
        }

        protected void AddLink(ActionLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException(nameof(link), "Reference to link cannot be null.");
            }
            _links.Add(link);
        }

        protected IResourceMeta GetResourceMeta(Type resourceType)
        {
            IResourceMeta resourceMeta = _resourceMap.ResourceMeta
                .FirstOrDefault(m => m.ResourceType == resourceType);

            if (resourceMeta == null)
            {
                throw new InvalidOperationException(
                    $"Resource Metadata has not yet been added for resource type: {resourceType.AssemblyQualifiedName} " + 
                    $"to the mapping of type: {_resourceMap.GetType().AssemblyQualifiedName}.");
            }
            return resourceMeta;
        }

        private static void AssertControllerMeetsConstraints(Type controllerType)
        {
            string[] duplicateActionNames = controllerType.GetActionMethods()
                .WhereDuplicated(am => am.Name)
                .ToArray();

            if (duplicateActionNames.Any())
            {
                throw new InvalidOperationException(
                    $"Resource linking requires all controller method names to be unique.  Controller: {controllerType.Name} " + 
                    $"has the following duplicated methods: {String.Join(" | ", duplicateActionNames)}");
            }
        }
    }
}
