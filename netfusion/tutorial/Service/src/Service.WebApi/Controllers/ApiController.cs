using NetFusion.Bootstrap.Refactors;

#pragma warning disable 4014
namespace Service.WebApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NetFusion.Bootstrap.Container;
    using NetFusion.Rest.Resources.Hal;
    using NetFusion.Rest.Server.Hal;
    using NetFusion.Web.Mvc.Metadata;
    using Service.Domain.Commands;
    using Service.Domain.Entities;
    using Service.Domain.Events;
    using Service.WebApi.Controllers.Core;
    using Service.WebApi.Controllers.Integration;

    [Route("api/entry"), GroupMeta(nameof(ApiController))]
    public class ApiController : Controller
    {
        private readonly ICompositeAppContainer _appContainer;
        
        public ApiController(ICompositeAppContainer appContainer)
        {
            _appContainer = appContainer ?? throw new ArgumentNullException(nameof(appContainer));
        }
        
        [HttpGet("examples")]
        public ExampleApiResource GetApiEntry()
        {
            return new ExampleApiResource();
        }

        [HttpGet("composite-log"), ActionMeta(nameof(GetCompositeLog))]
        public IActionResult GetCompositeLog()
        {
            return Ok(_appContainer.Log);
        }
    }

    public class ExampleApiResource : HalEntryPointResource
    {
        
    }

    public class ApiEntryPointRelations : HalResourceMap
    {
        public override void OnBuildResourceMap()
        {
            Map<ExampleApiResource>()
                .LinkMeta<ApiController>(meta =>
                {
                    meta.UrlTemplate<IActionResult>("composite-log", c => c.GetCompositeLog);
                })
                .LinkMeta<SettingsController>(meta =>
                {
                    meta.UrlTemplate<IActionResult>("injected-settings", c => c.ReadSettings);
                })
                .LinkMeta<MongoDbController>(meta =>
                {
                    meta.UrlTemplate<Customer, Task<IActionResult>>("add-customer", c => c.AddCustomer);
                    meta.UrlTemplate<string, Task<IActionResult>>("read-customer", c => c.ReadCustomer);
                    meta.UrlTemplate<string, Customer, Task<IActionResult>>("update-customer", c => c.UpdateCustomer);
                })
                .LinkMeta<RabbitMqController>(meta =>
                {
                    meta.UrlTemplate<PropertySold, Task>("rabbit-direct", c => c.RecordPropertySale);
                    meta.UrlTemplate<AutoSaleCompleted, Task>("rabbit-topic", c => c.RecordAutoSale);
                    meta.UrlTemplate<TemperatureReading, Task>("rabbit-fanout", c => c.TempReading);
                    meta.UrlTemplate<SendEmail, Task>("rabbit-queue", c => c.SendEmail);
                    meta.UrlTemplate<CalculatePropertyTax, Task<TaxCalc>>("rabbit-rpc-property",
                        c => c.CalculatePropertyTax);
                    meta.UrlTemplate<CalculateAutoTax, Task<TaxCalc>>("rabbit-rpc-auto", c => c.CalculateAutoTax);
                })
                .LinkMeta<RedisPublisherController>(meta =>
                {
                    meta.UrlTemplate<OrderSubmitted, Task>("redis-channel", c => c.SubmitOrder);
                })
                .LinkMeta<RedisDataController>(meta =>
                {
                    meta.UrlTemplate<RedisDataController.SetValue, Task>("set-value", c => c.AddValue);
                    meta.UrlTemplate<Task<RedisDataController.SetValue>>("pop-value", c => c.PopValue);
                })
                .LinkMeta<AmqpController>(meta =>
                {
                    meta.Url("amqp-send-command", (c, r) => c.SendClaimSubmission(null));
                    meta.Url("amqp-publish-event", (c, r) => c.PublishClaimStatus(null));
                });
        }
    }
}