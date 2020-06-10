using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Hal;
using Service.Domain.Commands;
using Service.Domain.Entities;
using Service.Domain.Events;
using Service.WebApi.Controllers.Core;
using Service.WebApi.Controllers.Integration;
using Service.WebApi.Controllers.Messaging;
using Service.WebApi.Models;

#pragma warning disable 4014
namespace Service.WebApi.Controllers
{
    [ApiController, Route("api/entry")]
    public class ApiController : ControllerBase
    {
        private readonly ICompositeApp _compositeApp;
        
        public ApiController(ICompositeApp compositeApp)
        {
            _compositeApp = compositeApp ?? throw new ArgumentNullException(nameof(compositeApp));
        }
        
        [HttpGet("examples")]
        public IActionResult GetApiEntry()
        {
            var entryPoint = new EntryPointModel();
            return Ok(entryPoint.AsResource());
        }

        [HttpGet("composite-log")]
        public IActionResult GetCompositeLog()
        {
            return Ok(_compositeApp.Log);
        }
    }


    public class ApiEntryPointRelations : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            Map<EntryPointModel>()
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
                .LinkMeta<CommandController>(meta =>
                {
                    meta.UrlTemplate<RangeModel, Task<IActionResult>>("messaging-command", c => c.GetRanges);
                })
                .LinkMeta<DomainEventController>(meta =>
                {
                    meta.UrlTemplate<AccountModel, Task<IActionResult>>("messaging-domain-event",
                        c => c.CreateAccount);
                })
                .LinkMeta<QueryController>(meta =>
                {
                    meta.UrlTemplate<int, Task<IActionResult>>("messaging-query", c => c.GetCustomerAddresses);
                })
                .LinkMeta<AmqpController>(meta =>
                {
                    meta.Url("amqp-send-command", (c, r) => c.SendClaimSubmission(null));
                    meta.Url("amqp-publish-event", (c, r) => c.PublishClaimStatus(null));
                })
                .LinkMeta<AttributedEntityController>(meta =>
                {
                    meta.Url("read-attributed-entity", (c, _) => c.ReadSensorData());
                    meta.Url("update-attributed-entity", (c, _) => c.AddAttributes(default));
                    meta.Url("dynamically-read-attributes", (c, _) => c.AccessDynamically());
                })
                .LinkMeta<RoslynController>(meta =>
                {
                    meta.Url("evaluate-sensor", (c, r) => c.EvaluateSensor(default));
                });
        }
    }
}