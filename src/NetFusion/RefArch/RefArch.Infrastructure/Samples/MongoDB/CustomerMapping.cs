using NetFusion.MongoDB;
using RefArch.Api.Models;

namespace RefArch.Infrastrcture
{
    public class CustomerMapping : EntityClassMap<Customer>
    {
        public CustomerMapping()
        {
            this.CollectionName = "RefArch.Customers";
            this.AutoMap();

            MapStringObjectIdProperty(c => c.CustomerId);
        }
    }
}
