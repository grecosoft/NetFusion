using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Messaging.Types.Attributes;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.WebApi.Controllers.Integration
{
    [ApiController, Route("api/integration/rabbitmq")]
    public class RabbitMqController : ControllerBase
    {
        private readonly IMessagingService _messaging;

        public RabbitMqController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }

        [HttpPost("property/sales")]
        public Task RecordPropertySale([FromBody]PropertySold propertyEvent)
        {
            propertyEvent.SetRouteKey(propertyEvent.State);
            return _messaging.PublishAsync(propertyEvent);
        }
        
        [HttpPost("auto/sales")]
        public Task RecordAutoSale([FromBody]AutoSaleCompleted salesCompleted)
        {
            salesCompleted.SetRouteKey(
                salesCompleted.Make,
                salesCompleted.Model,
                salesCompleted.Year);

            return _messaging.PublishAsync(salesCompleted);
        }
        
        [HttpPost("temp/reading")]
        public Task TempReading([FromBody]TemperatureReading reading)
        {
            return _messaging.PublishAsync(reading);
        }
        
        [HttpPost("send/email")]
        public Task SendEmail([FromBody]SendEmail command)
        {
            return _messaging.SendAsync(command);
        }
        
        [HttpPost("taxes/property")]
        public async Task<TaxCalc> CalculatePropertyTax([FromBody]CalculatePropertyTax command)
        {
            return await _messaging.SendAsync(command);
        }

        [HttpPost("taxes/auto")]
        public async Task<TaxCalc> CalculateAutoTax([FromBody]CalculateAutoTax command)
        {
            return await _messaging.SendAsync(command);
        }
    }
}