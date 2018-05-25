using Demo.App.Entities;
using NetFusion.Mapping;
using WebApiHost.Models;

namespace Demo.Infrastructure.Mappings
{
    public class CustomerContactMapping : MappingStrategy<Customer, CustomerSummary>
    {
        protected override CustomerSummary SourceToTarget(Customer source)
        {
            return new CustomerSummary
            {
                FullName = source.FirstName + " " + source.LastName,
                State = source.State,
                Zip =  source.Zip
            };
        }
    }
}
