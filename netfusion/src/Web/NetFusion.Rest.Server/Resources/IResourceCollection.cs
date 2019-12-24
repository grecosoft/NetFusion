// ReSharper disable UnusedTypeParameter
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Server.Resources
{
    /// <summary>
    /// Represents a collection of resources.  A collection of resources
    /// is also considered a resource and therefore implements IResource.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the collection.</typeparam>
    public interface IResourceCollection<TResource> : IResource
		where TResource : class, IResource
	{

	}
}
