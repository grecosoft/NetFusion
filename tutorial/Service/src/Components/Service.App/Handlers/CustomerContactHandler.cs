using System.Collections.Generic;
using System.Linq;
using NetFusion.Messaging;
using Service.Domain.Queries;

namespace Service.App.Handlers
{
    public class CustomerContactHandler : IQueryConsumer
    {
        private readonly List<Address> _addresses;

        public CustomerContactHandler()
        {
            _addresses = new List<Address>
            {
                new Address
                {
                    CustomerId = 10,
                    AddressLine1 = "1000 West Farms Road",
                    City = "Wallingford",
                    State = "CT",
                    Zip = "06471"
                },
                new Address
                {
                    CustomerId = 10,
                    AddressLine1 = "17 Main Street",
                    City = "Pittsburgh",
                    State = "PA",
                    Zip = "15063"
                },
                new Address
                {
                    CustomerId = 20,
                    AddressLine1 = "Deer Lane",
                    City = "Doylestown",
                    State = "PA",
                    Zip = "15964"
                }
            };
        }
        
        [InProcessHandler]
        public Address[] ListAddresses(QueryCustomerAddresses query)
        {
            return _addresses.Where(a => a.CustomerId == query.CustomerId).ToArray();
        }
    }
}