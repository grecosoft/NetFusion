using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
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
        Type DbContextType { get; }
        void SetDbContext(IEntityDbContext context);
    }

    /// <summary>
    /// Interface implemented by a class that delegates to the Entity Framework 
    /// database context.  
    /// This allows features to be easily added or hidden.
    /// </summary>
    /// <typeparam name="TEntityDbContext">The type of the data context.</typeparam>
    public interface IEntityContext<TEntityDbContext> : IEntityContext
        where TEntityDbContext : EntityDbContext
    {
        DbChangeTracker ChangeTracker { get; }
        DbContextConfiguration Configuration { get; }
        Database Database { get; }

        DbEntityEntry Entry(object entity);
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        IEnumerable<DbEntityValidationResult> GetValidationErrors();

        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        DbSet Set(Type entityType);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
