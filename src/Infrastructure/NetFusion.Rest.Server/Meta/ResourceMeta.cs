using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions.Collection;
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
    /// returned from the fluent methods.  This allows method chaining to be applied based on the derived type.</typeparam>
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
            Links = _links.ToReadOnly();

            _resourceMap = resourceMap;
        }

        /// <summary>
        /// Returns link metadata class used to specify controller action methods.
        /// </summary>
        /// <typeparam name="TController">The controller type so select action method from.</typeparam>
        /// <param name="meta">Method delegate passed metadata class used to define link metadata.</param>
        /// <returns>Reference to self for method chaining.</returns>
        public TResourceMeta LinkMeta<TController>(Action<ResourceLinkMeta<TController, TResource>> meta)
            where TController : Controller
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta), "Metadata delegate not specified.");

            AsertControllerMeetsConstraints(typeof(TController));

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
            if (meta == null) throw new ArgumentNullException(nameof(meta), "Metadata delegate not specified.");

            var resourceLinkMeta = new ResourceLinkMeta<TResource>();
            meta(resourceLinkMeta);

            AddLinks(resourceLinkMeta.GetActionLinks());
            return (TResourceMeta)this;
        }

        /// <summary>
        /// Applies the link metadata defined for one resource type to another.  This can be used when there
        /// are multiple representations for a resource type.  This is the case where there is a resource type
        /// and also different corresponding view-models.  When this is the case, often the links associated
        /// with the base resource type are also to be specified on the associated view-models.  This also 
        /// avoids from having to inherit resources and view-models from each other.
        /// </summary>
        /// <typeparam name="TFromResource">The type of an already mapped resource.</typeparam>
        /// <returns>Reference to self for method chaining.</returns>
        public TResourceMeta ApplyLinkMetaFrom<TFromResource>()
            where TFromResource: class, IResource
        {
            IResourceMeta resourceMeta = GetResourceMeta(typeof(TFromResource));
            AddValidLinks(resourceMeta);

            return (TResourceMeta)this;
        }

        // Creates a new resource mapping for a new resource type based on an existing resource type.
        // Only the links for which the new resource meets the criteria of the original resource type
        // are copied.  For example of the original link is based on a resource property of FooBarId, 
        // and the resource to which the link is being copied does not have such a property, the link
        // will not be assigned to the new resource type.
        private void AddValidLinks(IResourceMeta resourceMeta)
        {
            foreach (ActionLink actionLink in resourceMeta.Links)
            {
                if (actionLink.CanBeAppliedTo(typeof(TResource)))
                {
                    ActionLink newResourceLink = actionLink.CreateCopyFor<TResource>();

                    AddLink(newResourceLink);
                }
            }
        }

        protected void AddLinks(ActionLink[] links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links),
                    "Reference to link array cannot be null.");
            }
            _links.AddRange(links);
        }

        protected void AddLink(ActionLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException(nameof(link),
                    "Reference to link cannot be null.");
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
                    $"Resource Metadata does not exist or has not yet been added for resource type: {resourceType.AssemblyQualifiedName}");
            }
            return resourceMeta;
        }

        private void AsertControllerMeetsConstraints(Type controllerType)
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
