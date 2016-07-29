using NetFusion.Domain.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Scripting
{
    /// <summary>
    /// Implementation responsible for evaluating a script
    /// against a domain entity.
    /// </summary>
    public interface IEntityScriptingService
    {
        /// <summary>
        /// Loads the entity scripting service with a list of scripts.
        /// The implementation should pre-compile that expressions.
        /// </summary>
        /// <param name="scripts">List of entity associated scripts.</param>
        /// <param name="compiledOnLoad">Determines if all the script expressions should be
        /// compiled when loaded.  If this value is false, the expression will be compiled
        /// the first time it is used.</param>
        void Load(IEnumerable<EntityScript> scripts, bool compiledOnLoad = false);

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
        /// the script with the name Default is applied.  If a script name is applied, the
        /// default named script followed by the specified script is applied.</param>
        /// <returns>Future result that is completed after evaluation.</returns>
        Task Execute<TEntity>(TEntity entity, string scriptName = "default")
            where TEntity : class, IAttributedEntity;
    }
}
