using System.Collections.Generic;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// List based implementation for a resource collection.
    /// </summary>
    /// <typeparam name="TResouce">The type of resource contained within the collection.</typeparam>
    public class ResourceCollection<TResouce> : List<TResouce>,
		IResourceCollection<TResouce> where TResouce : class, IResource
	{
		public ResourceCollection(IEnumerable<TResouce> resources)
			: base(resources)
		{

		}
    }
}
