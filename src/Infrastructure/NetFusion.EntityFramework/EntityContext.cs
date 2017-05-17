using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NetFusion.Common;
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
    public class EntityContext<TEntityDbContext>: IEntityContext<TEntityDbContext>
        where TEntityDbContext : EntityDbContext
    {
        private TEntityDbContext _dbContext;

        public Type DbContextType => typeof(TEntityDbContext);

        public void SetDbContext(IEntityDbContext context)
        {
            Check.NotNull(context, nameof(context), "inner context not specified");

            _dbContext = (TEntityDbContext)context;
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

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
        }
    }
}
