namespace Service.WebApi.Controllers.Integration
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NetFusion.Redis;
    using NetFusion.Web.Mvc.Metadata;
    using StackExchange.Redis;

    [Route("api/integration/redis"),
     GroupMeta(nameof(MongoDbController))]
    public class RedisDataController : Controller
    {
        private readonly IDatabase _database;
        
        public RedisDataController(
            IRedisService redisService)
        {
            _database = redisService.GetDatabase("testdb");
        }
        
        [HttpPost("set/add"), ActionMeta(nameof(SetAddValue))]
        public Task SetAddValue([FromBody] SetValue setValue)
        {
            return _database.SetAddAsync("autos", setValue.Value);
        }

        [HttpGet("set/pop"), ActionMeta(nameof(SetAddValue))]
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