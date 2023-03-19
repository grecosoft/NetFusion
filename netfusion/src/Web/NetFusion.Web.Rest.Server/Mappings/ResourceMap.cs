using System;
using System.Collections.Generic;
using NetFusion.Web.Rest.Server.Meta;

namespace NetFusion.Web.Rest.Server.Mappings;

/// <summary>
/// Base class from which media-type specific mapping classes can derive.
/// Maintains a list of resource metadata added by the derived class.
/// </summary>
public abstract class ResourceMap : IResourceMap
{
    public IReadOnlyCollection<IResourceMeta> ResourceMeta { get; }
    
    private readonly List<IResourceMeta> _resourceMeta;

    protected ResourceMap()
    {
        _resourceMeta = new List<IResourceMeta>();
        ResourceMeta = _resourceMeta.AsReadOnly();
    }
    
    public abstract string MediaType { get; }
    public abstract Type ProviderType { get; }

    // Called by module during bootstrap to instruct derived class
    // to add resource mappings.
    void IResourceMap.BuildMap() => OnBuildResourceMap();
    
    /// <summary>
    /// Overridden by derived class to specify metadata associated returned resources. 
    /// </summary>
    protected abstract void OnBuildResourceMap();

    /// <summary>
    /// Adds an item containing metadata for a specific source type.
    /// </summary>
    /// <param name="resourceMeta">The resource metadata configured by derived map.</param>
    protected void AddResourceMeta(IResourceMeta resourceMeta)
    {
        if (resourceMeta == null) throw new ArgumentNullException(nameof(resourceMeta),
            "Resource metadata cannot be null.");
            
        _resourceMeta.Add(resourceMeta);
    }
}