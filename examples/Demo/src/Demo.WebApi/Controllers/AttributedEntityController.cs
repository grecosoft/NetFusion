using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Demo.Domain.Entities;

namespace Demo.WebApi.Controllers
{
    [ApiController, Route("api/attributed-entity")]
    public class AttributedEntityController : ControllerBase
    {
        [HttpPatch]
        public IActionResult UpdateEntity([FromBody]IDictionary<string, object> values)
        {
            var account = new Account("Justin", "Greco");
            
            // Example of adding set of values to entity:
            foreach (var (key, value) in values)
            {
                account.Attributes.SetValue(key, value);
            }
            
            // Example setting properties using .net dynamic runtime:
            account.Attributes.Values.AccountId = Guid.NewGuid().ToString();
            account.Attributes.Values.DateCreated = DateTime.UtcNow;
            
            // A namespace providing the context of a property can also be specified:
            account.Attributes.SetValue("RequestId", 
                Guid.NewGuid().ToString(),
                typeof(Account));
            
            return Ok(new
            {
                account,
                account.Attributes.Values.AccountId,
                dateCreated = account.Attributes.GetValue<DateTime>("DateCreated"),
                requestId = account.Attributes.GetValueOrDefault("RequestId",
                    "Not-Set",
                    typeof(Account)),
            });
        }
    }
}
