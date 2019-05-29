using NetFusion.Messaging.Types;

namespace Service.Domain.Queries
{
    public class QueryCustomerAddresses : Query<Address[]>
    {
        public int CustomerId { get; }
        
        public QueryCustomerAddresses(int customerId)
        {
            CustomerId = customerId;
        }
    }

    public class Address
    {
        public int CustomerId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}