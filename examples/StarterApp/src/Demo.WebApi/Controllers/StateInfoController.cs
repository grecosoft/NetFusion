using Microsoft.AspNetCore.Mvc;
using Demo.Infra.Repositories;
using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.Domain.Repositories;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class StateInfoController : Controller
    {
        private IStateInfoRepository StateInfoRep { get; }

        public StateInfoController(IStateInfoRepository stateInfoRep)
        {
            this.StateInfoRep = stateInfoRep;
        }

        [HttpPost]
        public Task<string> AddStateInfo([FromBody]StateInfo stateInfo)
        {
            return StateInfoRep.Add(stateInfo);
        }

        [HttpGet("{state}")]
        public Task<StateInfo> GetStateInfo(string state)
        {
            return StateInfoRep.Read(state);
        }
    }
}
