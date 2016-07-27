using NetFusion.Messaging;
using NetFusion.WebApi.Metadata;
using RefArch.Api.Commands;
using RefArch.Api.Models;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.Domain.Evaluation", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/domain/evaluation")]
    public class DomainEvaluationController : ApiController
    {
        private readonly IMessagingService _messagingSrv;

        public DomainEvaluationController(IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
        }

        [HttpPost, Route("dynamic", Name = "EvaluateDynamicEntity")]
        public async Task<EvaluatedDomainModel> EvaluateDynamicEntity(EvalDynamicEntityCommand command)
        {
            await _messagingSrv.PublishAsync(command);
            return command.Result;
        }
    }
}