using NetFusion.Domain.Entity.Services;
using NetFusion.Messaging;
using RefArch.Api.Commands;
using RefArch.Api.Models;
using System.Threading.Tasks;

namespace RefArch.Services.Domain
{
    public class EvalDomainService : IMessageConsumer
    {
        private readonly IEntityEvaluationService _evaluationSrv;

        public EvalDomainService(IEntityEvaluationService evaluationSrv)
        {
            _evaluationSrv = evaluationSrv;
        }

        [InProcessHandler]
        public async Task<EvaluatedDomainModel> OnMessage(EvalDynamicEntityCommand command)
        {
            var entity = new DynamicDomainEntity();
            entity.IsActive = command.IsActive;
            entity.MinValue = command.MinValue;
            entity.MaxValue = command.MaxValue;
 
            foreach (var attrib in command.Attributes) {
                entity.SetAttributeValue(attrib.Key, attrib.Value);
            }

            await _evaluationSrv.Evaluate(entity);
            return new EvaluatedDomainModel(entity);
        }
    }
}
