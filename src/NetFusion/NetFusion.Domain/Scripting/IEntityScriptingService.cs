using NetFusion.Domain.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Implementation responsible for evaluating a script against a domain entity
    /// and its optional set of dynamic attributes.
    /// </summary>
    public interface IEntityScriptingService
    {
        /// <summary>
        /// Loads the entity scripting service with a list of scripts.
        /// The implementation should pre-compile that expressions upon 
        /// first use.
        /// </summary>
        /// <param name="scripts">List of entity associated scripts.</param>
        void Load(IEnumerable<EntityScript> scripts);

        /// <summary>
        /// Compiles all script expressions that have not yet been compiled.
        /// </summary>
        void CompileAllScripts();

        /// <summary>
        /// Evaluates a script against an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity being evaluated.</typeparam>
        /// <param name="entity">The entity to have its state updated by applying the script.</param>
        /// <param name="scriptName">The optional script name to be executed.  If not specified,
        /// the script with the name 'default' is applied.  If a script name is specified, the
        /// default named script followed by the named script is applied.</param>
        /// <returns>Future result that is completed after evaluation.</returns>
        Task Execute<TEntity>(TEntity entity, string scriptName = "default")
            where TEntity : class;
    }
}
