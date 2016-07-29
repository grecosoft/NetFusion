using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Scripting
{
    public interface IEntityScriptRepository
    {
        /// <summary>
        /// Returns a list of all the configured entity scripts. 
        /// </summary>
        /// <returns>List of domain entity associated scripts.</returns>
        Task<IEnumerable<EntityScript>> ReadAll();
    }
}
