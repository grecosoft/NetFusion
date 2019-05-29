using NetFusion.MongoDB;
using Service.Domain.Entities;

namespace Service.Infra.Repositories.Mappings
{
    public class CustomerMapping : EntityClassMap<Customer>
    {
        public CustomerMapping()
        {
            CollectionName = "Contacts.Customers";
            AutoMap();

            MapStringPropertyToObjectId(c => c.CustomerId);
        }
    }
}