using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Documentation
{
    /// <summary>
    /// Provides access to action-method documentation.
    /// </summary>
    public interface IDocReader
    {
        /// <summary>
        /// Returns documentation for an action method.
        /// </summary>
        /// <param name="methodInfo">The controller action method being called.</param>
        /// <returns>Documentation Model for the action method.</returns>
        Task<DocActionModel> GetActionDocModel(IHeaderDictionary headers, MethodInfo methodInfo);
    }
}
