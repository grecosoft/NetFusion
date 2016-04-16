using NetFusion.Common;
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
    /// Class that delegates to the Entity Framework database context.  This
    /// allows features to be easily added or hidden without using inheritance.
    /// </summary>
    /// <typeparam name="TEntityDbContext">The type of the data context.</typeparam>
    public class EntityContext<TEntityDbContext>: IEntityContext<TEntityDbContext>
        where TEntityDbContext : EntityDbContext
    {
        private TEntityDbContext _dbContext;

        public Type DbContextType { get { return typeof(TEntityDbContext); } }

        public void SetDbContext(IEntityDbContext context)
        {
            Check.NotNull(context, nameof(context), "inner context not specified");

            _dbContext = context as TEntityDbContext;
        }

        public DbChangeTracker ChangeTracker
        {
            get { return _dbContext.ChangeTracker; }
        }

        public DbContextConfiguration Configuration
        {
            get { return _dbContext.Configuration; }
        }

        public Database Database
        {
            get { return _dbContext.Database; }
        }

        public DbEntityEntry Entry(object entity)
        {
            return _dbContext.Entry(entity);
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return _dbContext.Entry(entity);
        }

        public IEnumerable<DbEntityValidationResult> GetValidationErrors()
        {
            return _dbContext.GetValidationErrors();
        }

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return this.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return this.SaveChangesAsync(cancellationToken);
        }

        public DbSet Set(Type entityType)
        {
            return _dbContext.Set(entityType);
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _dbContext.Set<TEntity>();
        }
    }
}
