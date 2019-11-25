using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Base.Scripting;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScriptController : ControllerBase
    {
        private readonly IEntityScriptingService _scripting;
        private readonly IEntityScriptMetaRepository _scriptingRepo;

        public ScriptController(
            IEntityScriptingService scripting,
            IEntityScriptMetaRepository scriptingRepo)
        {
            _scripting = scripting;
            _scriptingRepo = scriptingRepo;
        }

        [HttpPost("apply/calcs")]
        public async Task<IDictionary<string, object>> ApplyCalcs([FromBody]StudentInfo model)
        {
            var student = new Student(
                model.FirstName, 
                model.LastName, 
                model.Scores);

            student.Attributes.Values.PassingScore = model.PassingScore;
            
            _scripting.Load(await _scriptingRepo.ReadAllAsync());
            _scripting.CompileAllScripts();

            await _scripting.ExecuteAsync(student, "scoreCalcs");

            var resultModel = new Dictionary<string, object>(student.AttributeValues);
            resultModel["Passing"] = student.Passing;
            return resultModel;
        }
    }
}