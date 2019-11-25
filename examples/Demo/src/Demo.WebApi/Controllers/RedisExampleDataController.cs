using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Redis;
using StackExchange.Redis;

namespace Demo.WebApi.Controllers 
{
    [Route("api/databaseexample")]
    [ApiController]
    public class RedisExampleDataController : ControllerBase
    {
        private readonly IDatabase _database;

        public RedisExampleDataController(
            IRedisService redis
        )
        {
            _database = redis.GetDatabase("testdb");
        }

        [HttpPost("set/add")]
        public Task SetAddValue([FromBody] SetValue setValue)
        {
            return _database.SetAddAsync("autos", setValue.Value);
        }

        [HttpGet("set/pop")]
        public async Task<string> SetPop()
        {
            var result = await _database.SetPopAsync("autos");
            return result;
        }

        public class SetValue
        {
            public string Value {get; set;}
        }
    }
}
