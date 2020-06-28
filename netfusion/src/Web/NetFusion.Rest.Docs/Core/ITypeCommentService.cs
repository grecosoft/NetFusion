using System;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.Core
{
    /// <summary>
    /// Service responsible for building a documentation
    /// for a specific model type based on an underlying
    /// source.
    /// </summary>
    public interface ITypeCommentService
    {
        /// <summary>
        /// Returns documentation for a controller action model type.
        /// </summary>
        /// <param name="resourceType">The type of the model to document.</param>
        /// <returns>Model containing documentation.</returns>
        ApiResourceDoc GetResourceDoc(Type resourceType);
    }
}