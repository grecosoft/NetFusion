using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Hal;
using Solution.Context.Domain.Commands;
using Solution.Context.Domain.Entities;
using Solution.Context.Domain.Queries;
using Solution.Context.WebApi.Resources;

namespace Solution.Context.WebApi.Controllers
{
    [ApiController, Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly IMessagingService _messaging;

        public CustomerController(
            IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationCommand registration)
        {
            if (registration == null) 
            {
                return BadRequest("Customer and Address not specified.");
            }

            Customer entity = await _messaging.SendAsync(registration);
            return Ok(CustomerResource.FromEntity(entity));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Lookup(Guid id, [FromQuery]bool includeSuggestions)
        {
            var query = new QueryCustomer(id)
            {
                IncludedSuggestions = includeSuggestions
            };

            CustomerInfo info = await _messaging.DispatchAsync(query);
            if (info == null) 
            {
                return NotFound("Contact not registered.");
            }

            return Ok(CustomerResource.FromEntity(info));
        }

        [HttpPut("{customerId}/addresses/{addressId}/primary")]
        public async Task<IActionResult> SetPrimaryAddress(Guid customerId, Guid addressId)
        {
            var command = new SetPrimaryAddressCommand(customerId, addressId);

            await _messaging.SendAsync(command);
            return Ok();
        }

        #pragma warning disable CS4014
        public class CustomerMappings : HalResourceMap
        {
            protected override void OnBuildResourceMap()
            {
                Map<CustomerResource>()
                    .LinkMeta<CustomerController>(meta =>
                    {
                        meta.Url(RelationTypes.Self, (c, r) => c.Lookup(r.Id, default));
                    });
                
                Map<AddressResource>()
                    .LinkMeta<CustomerController>(meta =>
                    {
                        meta.Url("set-primary", (c, r) => c.SetPrimaryAddress(r.CustomerId, r.Id));
                    });
                
                Map<AutoResource>()
                    .LinkMeta(meta =>
                    {
                        meta.Href("set-primary", HttpMethod.Get, 
                            r => $"http://wwww.autos.com/makes/{r.Make}/{r.Model}/{r.Year}");
                    });
            }
        }
    }
}