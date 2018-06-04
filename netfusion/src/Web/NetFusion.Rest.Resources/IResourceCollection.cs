// ReSharper disable UnusedTypeParameter
namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Represents a collection of resources.
    /// </summary>
    /// <typeparam name="TResouce">The type of resource contained within the collection.</typeparam>
    public interface IResourceCollection<TResouce> : IResource
		where TResouce : class, IResource
	{

	}
}
