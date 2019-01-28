using Microsoft.EntityFrameworkCore;

namespace NetFusion.EntityFramework.Internal
{
    /// <summary>
    /// Derived by classes containing mapping logic for one or more entities.
    /// </summary>
    public abstract class EntityTypeMapping : IEntityTypeMapping
    {
        public abstract void AddMappings(ModelBuilder modelBuilder);
    }
}