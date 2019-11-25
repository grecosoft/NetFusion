using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Demo.Domain.Adapters;

namespace Demo.Infra
{
    public class RegistrationDataAdapter : IRegistrationDataAdapter
    {
        private readonly ILogger _logger;

        public RegistrationDataAdapter(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Registration Adapter");
        }

        public async Task<AutoInfo[]> GetValidModelsAsync(int forYear)
        {
           _logger.LogDebug("Attempting to download data...");

            var httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(
                @"https://raw.githubusercontent.com/grecosoft/NetFusion/master/examples/data/valid_autos.json");

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogDebug(responseBody);

            var data = JsonConvert.DeserializeObject<AutoRegDataResponse>(responseBody);
            return data.AutoInfo.Where(a => a.Year == forYear).ToArray();
        }

        private class AutoRegDataResponse
        {
            public AutoInfo[] AutoInfo { get; set; }
        }
    }
}
