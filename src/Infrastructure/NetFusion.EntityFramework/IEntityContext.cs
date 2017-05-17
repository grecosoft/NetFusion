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
        /// <param name="context"></param>
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
        ChangeTracker ChangeTracker { get; }
        DatabaseFacade Database { get; }

        EntityEntry Entry(object entity);
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
