using System.Threading.Tasks;

namespace NetFusion.Domain.Entity.Services
{
    public interface IEntityEvaluationService
    {
        Task Load();

        Task ApplyExpressions<TEntity>(TEntity entity)
            where TEntity : IAttributedEntity;
    }
}
