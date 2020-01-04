using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.Domain.Repositories;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateInfoController : ControllerBase
    {
        private IStateInfoRepository StateInfoRep { get; }

        public StateInfoController(IStateInfoRepository stateInfoRep)
        {
            StateInfoRep = stateInfoRep;
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
