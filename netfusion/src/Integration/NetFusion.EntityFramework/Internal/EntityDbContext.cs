using System;
using Microsoft.EntityFrameworkCore;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.EntityFramework.Internal
{
    /// <summary>
    /// Derived instance of the EntityFramework DbContext initialized from information founed by the
    /// plugin when bootstrapped.  Instances of this class can be injected into application specific
    /// components (i.e. Repositories) using the application defined derived IEntityDbContext contract.
    /// </summary>
    public abstract class EntityDbContext : DbContext,
        IEntityDbContext
    {
        private readonly string _connectionString;
        private readonly IEntityTypeMapping[] _mappings;

        public DbContext Context => this;
        
        protected EntityDbContext(string connectionString, IEntityTypeMapping[] mappings)
        {
            _connectionString = connectionString ?? 
                throw new ArgumentNullException(nameof(connectionString));

            _mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Implemented by the derived application context class where it can build the
            // database specific connection object from the provided connection string.
            ConfigureDbServer(optionsBuilder, _connectionString);
           
        }
        
        // Add the entity-to-database mappings discovered by the plugin to the context.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            _mappings.ForEach(m => m.AddMappings(modelBuilder));
        }

        /// <summary>
        /// Implemented by the derived application specific class to build a database specific
        /// connection object from the provided connection string.
        /// </summary>
        /// <param name="optionsBuilder">The builder used to provide cotext options.</param>
        /// <param name="connectionString">The configuration string assocated with the context.</param>
        protected abstract void ConfigureDbServer(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}