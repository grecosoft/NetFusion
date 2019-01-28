namespace Service.Infra.Repositories.Mappings
{
    using NetFusion.MongoDB;
    using Service.Domain.Entities;

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