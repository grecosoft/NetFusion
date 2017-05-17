using Microsoft.EntityFrameworkCore;
using NetFusion.Base.Plugins;

namespace NetFusion.EntityFramework
{
    /// <summary>
    /// Interface used to identify class instances that are used to 
    /// provided mappings between entities and database tables.
    /// </summary>
    public interface IEntityTypeMapping : IKnownPluginType
    {
        /// <summary>
        /// Called during the module bootstrap process to add
        /// the mapping to the database context.
        /// </summary>
        /// <param name="modelBuilder">The model builder associated with the context.</param>
        void AddMappings(ModelBuilder modelBuilder);
    }
}
