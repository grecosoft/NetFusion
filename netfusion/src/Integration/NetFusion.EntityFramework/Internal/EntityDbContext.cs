using System;
using Microsoft.EntityFrameworkCore;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.EntityFramework.Internal
{
    using NetFusion.EntityFramework.Settings;

    /// <summary>
    /// Derived instance of the EntityFramework DbContext initialized from information found by the
    /// plugin when bootstrapped.  Instances of this class can be injected into application specific
    /// components (i.e. Repositories) using the application defined derived IEntityDbContext contract.
    /// </summary>
    public abstract class EntityDbContext : DbContext,
        IEntityDbContext
    {
        private readonly DbContextSettings _contextSettings;
        private readonly IEntityTypeMapping[] _mappings;

        public DbContext Context => this;
        
        protected EntityDbContext(DbContextSettings contextSettings, IEntityTypeMapping[] mappings)
        {
            _contextSettings = contextSettings ?? throw new ArgumentNullException(nameof(contextSettings));
            _mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Allow derived class to configure context based on associated settings.
            ConfigureDbContext(optionsBuilder, _contextSettings);
        }
        
        /// <summary>
        /// Override this method to configure the database (and other options) to be used for this context.
        /// </summary>
        /// <param name="optionsBuilder">The builder used to provide context options.</param>
        /// <param name="contextSettings">The settings associated with the context.</param>
        protected abstract void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, DbContextSettings contextSettings);
        
        // Adds mappings to the context.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _mappings.ForEach(m => m.AddMappings(modelBuilder));
        }        
    }
}