using NetFusion.Rest.Server.Meta;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Base class from which REST specific mapping classes can derive.
    /// Maintains a list of resource metadata added by the derived class.
    /// </summary>
    public abstract class ResourceMap : IResourceMap
    {
        public Type ProviderType { get; private set; }
        private readonly List<IResourceMeta> _resourceMeta;

        protected ResourceMap()
        {
            _resourceMeta = new List<IResourceMeta>();
            ResourceMeta = _resourceMeta.AsReadOnly();
        }

        public IReadOnlyCollection<IResourceMeta> ResourceMeta { get; }
        public abstract string MediaType { get; }

        // Called by module during bootstrap to instruct derived class
        // to add resource mappings.
        void IResourceMap.BuildMap()
        {
            OnBuildResourceMap();
        }

        protected abstract void OnBuildResourceMap();

        /// <summary>
        /// Add an item containing metadata for a specific source type.
        /// </summary>
        /// <param name="resourceMeta">The resource metadata configured by derived map.</param>
        protected void AddResourceMeta(IResourceMeta resourceMeta)
        {
            if (resourceMeta == null) throw new ArgumentNullException(nameof(resourceMeta),
                "Resource metadata cannot be null.");
            
            _resourceMeta.Add(resourceMeta);
        }

        /// <summary>
        /// Used by derived classes to set the provider used to process the resource
        /// metadata associated with resources. 
        /// </summary>
        /// <typeparam name="TProvider">The provider type.</typeparam>
        protected void SetProvider<TProvider>()
            where TProvider : IResourceProvider
        {
            ProviderType = typeof(TProvider);
        }
    }
}
