using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.EntityFramework
{
    /// <summary>
    /// Class that delegates to the Entity Framework database context.  This
    /// allows features to be easily added or hidden without using inheritance.
    /// </summary>
    /// <typeparam name="TEntityDbContext">The type of the data context.</typeparam>
    public class EntityContext<TEntityDbContext>: IEntityContext<TEntityDbContext>, IDisposable
        where TEntityDbContext : EntityDbContext
    {
        private TEntityDbContext _dbContext;
        private bool _disposed = false;

        public Type DbContextType => typeof(TEntityDbContext);

        public void SetDbContext(IEntityDbContext context)
        {
            _dbContext = (TEntityDbContext)context ?? throw new ArgumentNullException(nameof(context), 
                "Inner context cannot be null.");
        }

        public ChangeTracker ChangeTracker
        {
            get { return _dbContext.ChangeTracker; }
        }

        public DatabaseFacade Database
        {
            get { return _dbContext.Database; }
        }

        public EntityEntry Entry(object entity)
        {
            return _dbContext.Entry(entity);
        }

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return _dbContext.Entry(entity);
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
        }

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        public int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return _dbContext.SaveChanges(acceptAllChangesOnSuccess);
        }

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;

            _dbContext?.Dispose();

            _disposed = true;
        }
    }
}
