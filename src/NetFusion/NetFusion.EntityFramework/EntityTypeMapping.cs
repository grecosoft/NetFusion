using NetFusion.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace NetFusion.EntityFramework
{
    /// <summary>
    /// Class deriving from the Entity Framework base type configuration where helper
    /// methods can be added.  Also, introduces a non-generic interface so the mapping
    /// can be easily referenced.
    /// </summary>
    /// <typeparam name="TEntityType">The entity corresponding to the mapping.</typeparam>
    public abstract class EntityTypeMapping<TEntityType> : EntityTypeConfiguration<TEntityType>,
        IEntityTypeMapping
        where TEntityType : class
    {
        public void AddMapping(DbModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            modelBuilder.Configurations.Add(this);
        }
    }
}


