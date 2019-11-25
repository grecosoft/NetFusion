﻿using System.Collections.Generic;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// List based implementation for a resource collection.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the collection.</typeparam>
    public class ResourceCollection<TResource> : List<TResource>,
		IResourceCollection<TResource> where TResource : class, IResource
	{
		public ResourceCollection(IEnumerable<TResource> resources)
			: base(resources)
		{

		}
    }
}
