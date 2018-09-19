using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Redis;
using StackExchange.Redis;

namespace Demo.WebApi.Controllers 
{
    [Route("api/[controller]")]
    public class DatabaseExampleController : Controller
    {
        private readonly IRedisService _redis;
        private readonly IDatabase _database;
        private readonly ISubscriber _subscriber;

        public DatabaseExampleController(
            IRedisService redis
        )
        {
            _redis = redis ?? throw new System.ArgumentNullException(nameof(redis));
            _database = redis.GetDatabase("testdb");
            _subscriber = redis.GetSubscriber("testdb");
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