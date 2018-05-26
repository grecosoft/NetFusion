using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Demo.Infra
{
    public class SalesDataAdapter : ISalesDataAdapter
    {
        private readonly ILogger _logger;

        public SalesDataAdapter(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Registration Adapter");
        }

        public async Task<AutoSalesInfo[]> GetInventory(string make, int year)
        {
            _logger.LogDebug("Attempting to download data...");

            var httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(
                @"https://raw.githubusercontent.com/grecosoft/NetFusion/master/examples/data/invertory.json");
                
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogDebug(responseBody);

            var data = JsonConvert.DeserializeObject<InventoryResponse>(responseBody);
            return data.SalesInfo.Where(s => s.Make == make && s.Year == year).ToArray();
        }

        private class InventoryResponse
        {
            public AutoSalesInfo[] SalesInfo { get; set; }
        }
    }
}