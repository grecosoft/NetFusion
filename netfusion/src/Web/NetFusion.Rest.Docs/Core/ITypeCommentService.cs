using System;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.Core
{
    /// <summary>
    /// Service responsible for building  documentation for a specific type based on an underlying source.
    /// </summary>
    public interface ITypeCommentService
    {
        /// <summary>
        /// Returns documentation for a resource type.
        /// </summary>
        /// <param name="resourceType">The type of the resource to document.</param>
        /// <returns>Model containing documentation for the resource.</returns>
        ApiResourceDoc GetResourceDoc(Type resourceType);
    }
}