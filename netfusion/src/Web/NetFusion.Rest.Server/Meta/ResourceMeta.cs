using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Rest.Server.Linking;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Metadata associated with resource.
    /// </summary>
    /// <typeparam name="TResourceMeta">Derived metadata class containing specific metadata.  This is the type
    /// returned from the fluent methods.  This allows method chaining to be applied based on the derived type.
    /// </typeparam>
    /// <typeparam name="TSource">The type of resource associated with the metadata.</typeparam>
    public class ResourceMeta<TResourceMeta, TSource> : IResourceMeta
	    where TSource : class
		where TResourceMeta : ResourceMeta<TResourceMeta, TSource>
    {
        public Type SourceType => typeof(TSource);
       
        /// <summary>
        /// Links associated with the resource.  Usually used to navigate additional
        /// associated resources or invoke action on current resource.
        /// </summary>
        public IReadOnlyCollection<ResourceLink> Links { get; }

        private readonly List<ResourceLink> _links = new List<ResourceLink>();

        protected ResourceMeta()
        {
            Links = _links.AsReadOnly();
        }

        /// <summary>
        /// Returns link metadata class used to specify controller action methods.
        /// </summary>
        /// <typeparam name="TController">The controller type so select action method from.</typeparam>
        /// <param name="meta">Method delegate passed metadata class used to define link metadata.</param>
        /// <returns>Reference to self for method chaining.</returns>
        public TResourceMeta LinkMeta<TController>(Action<ResourceLinkMeta<TController, TSource>> meta)
            where TController : ControllerBase
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta),
                "Metadata delegate cannot be null.");

            AssertControllerMeetsConstraints(typeof(TController));

            var resourceLinkMeta = new ResourceLinkMeta<TController, TSource>();
            meta(resourceLinkMeta);

            AddLinks(resourceLinkMeta.GetResourceLinks());
            return (TResourceMeta)this;
        }

        /// <summary>
        /// Returns link metadata class used to specify URIs not associated with a specific controller method.
        /// </summary>
        /// <param name="meta">Method delegate passed metadata class used to define link metadata.</param>
        /// <returns>Reference to self for method chaining.</returns>
        public TResourceMeta LinkMeta(Action<ResourceLinkMeta<TSource>> meta)
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta),
                "Metadata delegate not specified.");

            var resourceLinkMeta = new ResourceLinkMeta<TSource>();
            meta(resourceLinkMeta);

            AddLinks(resourceLinkMeta.GetResourceLinks());
            return (TResourceMeta)this;
        }

        protected void AddLinks(ResourceLink[] links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links), "Reference to link array cannot be null.");
            }
            _links.AddRange(links);
        }

        protected void AddLink(ResourceLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException(nameof(link), "Reference to link cannot be null.");
            }
            _links.Add(link);
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
                    $"has the following duplicated methods: {string.Join(" | ", duplicateActionNames)}");
            }
        }
    }
}
