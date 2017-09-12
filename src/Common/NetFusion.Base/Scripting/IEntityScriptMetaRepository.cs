using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Base.Scripting
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

        /// <summary>
        /// Saves a new script or updates an existing.
        /// </summary>
        /// <param name="script">The script to same.</param>
        /// <returns>The existing identity value or the newly generated value.</returns>
        Task<string> SaveAsync(EntityScript script);
    }
}
