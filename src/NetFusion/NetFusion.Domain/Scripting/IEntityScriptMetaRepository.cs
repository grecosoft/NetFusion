using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Responsible for maintaining a set of entity related scripts that can
    /// be executed at runtime against the entity.
    /// </summary>
    public interface IEntityScriptMetaRepository
    {
        /// <summary>
        /// Returns a list of all the configured entity scripts. 
        /// </summary>
        /// <returns>List of domain entity associated scripts.</returns>
        Task<IEnumerable<EntityScript>> ReadAll();
    }
}
