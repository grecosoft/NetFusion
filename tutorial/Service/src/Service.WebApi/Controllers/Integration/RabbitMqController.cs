using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.WebApi.Controllers.Integration
{
    [Route("api/integration/rabbitmq"),
     GroupMeta(nameof(MongoDbController))]
    public class RabbitMqController : Controller
    {
        private readonly IMessagingService _messaging;

        public RabbitMqController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("property/sales"), ActionMeta(nameof(RecordPropertySale))]
        public Task RecordPropertySale([FromBody]PropertySold propertyEvent)
        {
            propertyEvent.SetRouteKey(propertyEvent.State);
            return _messaging.PublishAsync(propertyEvent);
        }
        
        [HttpPost("auto/sales"), ActionMeta(nameof(RecordAutoSale))]
        public Task RecordAutoSale([FromBody]AutoSaleCompleted salesCompleted)
        {
            salesCompleted.SetRouteKey(
                salesCompleted.Make,
                salesCompleted.Model,
                salesCompleted.Year);

            return _messaging.PublishAsync(salesCompleted);
        }
        
        [HttpPost("temp/reading"), ActionMeta(nameof(TempReading))]
        public Task TempReading([FromBody]TemperatureReading reading)
        {
            return _messaging.PublishAsync(reading);
        }
        
        [HttpPost("send/email"), ActionMeta(nameof(SendEmail))]
        public Task SendEmail([FromBody]SendEmail command)
        {
            return _messaging.SendAsync(command);
        }
        
        [HttpPost("taxes/property"), ActionMeta(nameof(CalculatePropertyTax))]
        public async Task<TaxCalc> CalculatePropertyTax([FromBody]CalculatePropertyTax command)
        {
            return await _messaging.SendAsync(command);
        }

        [HttpPost("taxes/auto"), ActionMeta(nameof(CalculateAutoTax))]
        public async Task<TaxCalc> CalculateAutoTax([FromBody]CalculateAutoTax command)
        {
            return await _messaging.SendAsync(command);
        }
    }
}