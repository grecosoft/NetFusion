using System;
using System.Threading.Tasks;
using Demo.App.DomainEvents;
using Demo.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SampleTopicController : Controller
    {
        private readonly IMessagingService _messaging;
        
        public SampleTopicController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }
        
        [HttpPost("auto/sales")]
        public Task RecordAutoSale([FromBody]AutoModel model)
        {
            var salesCompleted = new AutoSaleCompleted(
                model.Make,
                model.Model,
                model.Year,
                model.Color)
            {
                IsNew = model.IsNew
            };

            return _messaging.PublishAsync(salesCompleted);
        }
    }
}