using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Generation
{
    /// <summary>
    /// Determines the resources used for a controller's action method and returns
    /// the corresponding type-script definitions.
    /// </summary>
    public interface IResourceTypeReader
    {
        /// <summary>
        /// Returns type-script definitions for a given controller action method.
        /// </summary>
        /// <param name="actionMethodInfo">The method information for the controller's action method.</param>
        /// <returns>Scream containing the corresponding type-script definitions.</returns>
        Task<MemoryStream> ReadTypeDefinitionFiles(MethodInfo actionMethodInfo);

        Task<MemoryStream> ReadTypeDefinitonFile(Type resourceType);
    }
}
