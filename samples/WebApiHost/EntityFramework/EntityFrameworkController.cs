using Microsoft.AspNetCore.Mvc;
using NetFusion.Web.Mvc.Metadata;
using System.Threading.Tasks;
using WebApiHost.EntityFramework.Models;

namespace WebApiHost.EntityFramework
{
    [Route("api/[controller]")]
    [GroupMeta("EntityFramework")]
    public class EntityFrameworkController : Controller
    {
        private readonly IAddressRepository _addressRep;

        public EntityFrameworkController(
            IAddressRepository addressRep)
        {
            _addressRep = addressRep;
        }

        [HttpGet("address")]
        [ActionMeta("ListAddresses")]
        public Task<Address[]> ListAddresses()
        {
            return _addressRep.ReadAddresses();
        }

        [HttpGet("test-method/item/{id}/{status}"), ActionMeta("Item-Status")]
        public void TestMethod(string id, string status = "IMPORTANT")
        {

        }
    }
}
