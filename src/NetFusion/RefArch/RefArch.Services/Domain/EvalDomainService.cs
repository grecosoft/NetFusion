using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using RefArch.Api.Commands;
using RefArch.Api.Models;
using System.Threading.Tasks;

namespace RefArch.Services.Domain
{
    public class EvalDomainService : IMessageConsumer
    {
        private readonly IEntityScriptingService _evaluationSrv;

        public EvalDomainService(IEntityScriptingService evaluationSrv)
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
 
            foreach (var attrib in command.AttributeValues) {
                entity.Attributes.SetValue(attrib.Key, attrib.Value);
            }

            await _evaluationSrv.ExecuteAsync(entity, scriptName: "Test-Script");
            return new EvaluatedDomainModel(entity);
        }
    }
}
