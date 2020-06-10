using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Redis;
using StackExchange.Redis;

namespace Service.WebApi.Controllers.Integration
{
    [ApiController, Route("api/integration/redis")]
    public class RedisDataController : ControllerBase
    {
        private readonly IDatabase _database;
        
        public RedisDataController(
            IRedisService redisService)
        {
            _database = redisService.GetDatabase("testdb");
        }
        
        [HttpPost("set/add")]
        public Task AddValue([FromBody] SetValue setValue)
        {
            return _database.SetAddAsync("autos", setValue.Value);
        }

        [HttpGet("set/pop")]
        public async Task<SetValue> PopValue()
        {
            var result = await _database.SetPopAsync("autos");
            return new SetValue
            {
                Value = result
            };
        }

        public class SetValue
        {
            public string Value {get; set;}
        }
    }
}