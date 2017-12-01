using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NetFusion.Base.Plugins;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.EntityFramework
{
    /// <summary>
    /// Interface implemented by a class that delegates to the Entity Framework
    /// database context. 
    /// </summary>
    public interface IEntityContext : IKnownPluginType
    {
        /// <summary>
        /// The type of the derived Entity Framework DBContext.
        /// </summary>
        Type DbContextType { get; }

        /// <summary>
        /// Sets the underlying Entity Framework context instance to which is delegated.
        /// </summary>
        /// <param name="context">Entity Framework derived context.</param>
        void SetDbContext(IEntityDbContext context);
    }

    /// <summary>
    /// Interface implemented by a class that delegates to the Entity Framework 
    /// database context.  This allows features to be easily added or hidden.
    /// </summary>
    /// <typeparam name="TEntityDbContext">The type of the data context.</typeparam>
    public interface IEntityContext<TEntityDbContext> : IEntityContext
        where TEntityDbContext : EntityDbContext
    {
        /// <summary>
        ///  Provides access to information and operations for entity instances this context is tracking.
        /// </summary>
        ChangeTracker ChangeTracker { get; }

        /// <summary>
        /// Provides access to database related information and operations for this context.
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// Gets an Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry for the given
        /// entity. The entry provides access to change tracking information and operations
        /// for the entity.
        /// This method may be called on an entity that is not tracked. You can then set
        /// the Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry.State property on
        /// the returned entry to have the context begin tracking the entity in the specified
        /// </summary>
        /// <param name="entity">The entity to get the entry for.</param>
        /// <returns>The entry for the given entity.</returns>
        EntityEntry Entry(object entity);

        /// <summary>
        /// Gets an Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry`1 for the given
        ///  entity. The entry provides access to change tracking information and operations
        ///  for the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to get the entry for.</param>
        /// <returns>The entry for the given entity.</returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        /// <remarks>
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database.
        /// This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        /// </remarks>
        int SaveChanges();

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges
        /// is called after the changes have been sent successfully to the database.</param>
        /// <returns>The number of state entries written to the database.</returns>
        /// <remarks>
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        ///  to discover any changes to entity instances before saving to the underlying database.
        ///  This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        /// </remarks>
        int SaveChanges(bool acceptAllChangesOnSuccess);

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges
        /// is called after the changes have been sent successfully to the database.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        /// <remarks>
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database.
        /// This can be disabled via Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled.
        /// Multiple active operations on the same context instance are not supported. Use 'await' to ensure that any 
        /// asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken"> A System.Threading.CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///  Creates a Microsoft.EntityFrameworkCore.DbSet`1 that can be used to query and save instances of TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which a set should be returned.</typeparam>
        /// <returns>A set for the given entity type.</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
