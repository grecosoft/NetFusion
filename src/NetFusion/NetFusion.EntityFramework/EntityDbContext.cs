using NetFusion.Common;
using NetFusion.Common.Extensions;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NetFusion.EntityFramework
{
    /// <summary>
    /// Simple class deriving from the base Entity Framework database context
    /// allowing common implementations.
    /// </summary>
    public class EntityDbContext : DbContext,
        IEntityDbContext
    {
        private readonly IEnumerable<IEntityTypeMapping> _mappings;

        public EntityDbContext(string connection, IEnumerable<IEntityTypeMapping> mappings)
            : base(connection)
        {
            Check.NotNull(mappings, nameof(mappings), "entity mappings not specified");

            _mappings = mappings;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _mappings.Where(IsWithinContextNamespace).
                ForEach(m => m.AddMapping(modelBuilder));
        }

        private bool IsWithinContextNamespace(IEntityTypeMapping mapping)
        {
            return mapping.GetType().Namespace.StartsWith(
                this.GetType().Namespace, System.StringComparison.Ordinal);
        }
    }
}
